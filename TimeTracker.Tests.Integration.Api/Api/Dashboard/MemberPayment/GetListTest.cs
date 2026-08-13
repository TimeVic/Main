using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.MemberPayment;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/member-payments/list";
    
    private readonly UserEntity _user;
    private new readonly IDataFactory<MemberPaymentEntity> _factory;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ClientEntity _client;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ProjectEntity _project;
    private readonly IMemberPaymentSeeder _paymentSeeder;
    private readonly IUserSeeder _userSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<MemberPaymentEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IMemberPaymentSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_workspace).Wait();

        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _client = _project.Client;
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task UserCanNotGetListInSoloWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1
        });
        
        var actual = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), actual.ErrorCode);
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedTotal = 21;
        await _paymentSeeder.CreateSeveralAsync(_workspace, _user, _project, expectedTotal);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedTotal, actualResponse.TotalCount);
        
        Assert.All(actualResponse.Items, item =>
        {
            Assert.NotNull(item.Project);
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.Equal(_client.Id, item.Client!.Id);
            Assert.Equal(_project.Id, item.Project.Id);
            Assert.Equal(_user.Id, item.Member.User.Id);
            Assert.True(item.Amount > 0);
            Assert.NotEmpty(item.Description!);
            Assert.True(item.PaymentTime > DateTime.MinValue);
        });
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfHasNoAccess()
    {
        var (otherJwtToken, otherUser, otherWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        
        var response = await PostRequestAsync(Url, otherJwtToken, new GetListRequest()
        {
            Page = 1
        }, _workspace.Id);
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), errorResponse.ErrorCode);
    }
    
    [Fact]
    public async Task ShouldReceiveOnlyForCurrentUser()
    {
        var (userJwt, workspaceUser, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        var (_, otherUser, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        await FlushDbChanges();
        await _paymentSeeder.CreateSeveralAsync(_workspace, otherUser, _project, 5);
        
        var expectedTotal = 21;
        await _paymentSeeder.CreateSeveralAsync(_workspace, workspaceUser, _project, expectedTotal);
        
        var response = await PostRequestAsync(Url, userJwt, new GetListRequest()
        {
            Page = 1
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedTotal, actualResponse.TotalCount);
        Assert.All(actualResponse.Items, item => Assert.Equal(workspaceUser.Id, item.Member.User.Id));
    }

    [Fact]
    public async Task UserCanNotFilterByWorkspaceMember()
    {
        var (userJwt, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        var memberUser = await _userSeeder.CreateActivatedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        await FlushDbChanges();
        var member = _workspace.Members.First(item => item.User.Id == memberUser.Id);

        var response = await PostRequestAsync(Url, userJwt, new GetListRequest()
        {
            Page = 1,
            MemberId = member.Id
        }, _workspace.Id);

        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
    }

    [Fact]
    public async Task ManagerShouldReceivePaymentsForAllWorkspaceMembers()
    {
        var (managerJwt, managerUser, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );
        await FlushDbChanges();
        await _paymentSeeder.CreateSeveralAsync(_workspace, _user, _project, 3);
        await _paymentSeeder.CreateSeveralAsync(_workspace, managerUser, _project, 4);

        var response = await PostRequestAsync(Url, managerJwt, new GetListRequest()
        {
            Page = 1
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(7, actualResponse.TotalCount);
    }

    [Fact]
    public async Task ManagerShouldFilterPaymentsByWorkspaceMember()
    {
        var (managerJwt, managerUser, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );
        await FlushDbChanges();
        var managerMember = _workspace.Members.First(item => item.User.Id == managerUser.Id);
        await _paymentSeeder.CreateSeveralAsync(_workspace, _user, _project, 3);
        await _paymentSeeder.CreateSeveralAsync(_workspace, managerUser, _project, 4);

        var response = await PostRequestAsync(Url, managerJwt, new GetListRequest()
        {
            Page = 1,
            MemberId = managerMember.Id
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(4, actualResponse.TotalCount);
        Assert.All(actualResponse.Items, item => Assert.Equal(managerUser.Id, item.Member.User.Id));
    }
}
