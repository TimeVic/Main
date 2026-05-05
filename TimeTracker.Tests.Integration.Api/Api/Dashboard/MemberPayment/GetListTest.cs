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
    private readonly IClientDao _clientDao;
    private readonly WorkspaceEntity _workspace;
    private readonly ClientEntity _client;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly IMemberPaymentSeeder _paymentSeeder;
    private readonly IUserSeeder _userSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<MemberPaymentEntity>>();
        _clientDao = ServiceProvider.GetRequiredService<IClientDao>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IMemberPaymentSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _client = _clientDao.CreateAsync(_workspace, "Test new client").Result;
        _project = _projectDao.CreateAsync(_workspace, "Test new project").Result;
        _project.SetClient(_client);
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedTotal = 21;
        await _paymentSeeder.CreateSeveralAsync(_workspace, _user, _client, _project, expectedTotal);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedTotal, actualResponse.TotalCount);
        
        Assert.All(actualResponse.Items, item =>
        {
            Assert.NotNull(item.Project);
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.Equal(_client.Id, item.Client.Id);
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
            WorkspaceId = _workspace.Id,
            Page = 1
        });
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
        await _paymentSeeder.CreateSeveralAsync(_workspace, otherUser, _client, _project, 5);
        
        var expectedTotal = 21;
        await _paymentSeeder.CreateSeveralAsync(_workspace, workspaceUser, _client, _project, expectedTotal);
        
        var response = await PostRequestAsync(Url, userJwt, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1
        });
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
            WorkspaceId = _workspace.Id,
            Page = 1,
            MemberId = member.Id
        });

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
        await _paymentSeeder.CreateSeveralAsync(_workspace, _user, _client, _project, 3);
        await _paymentSeeder.CreateSeveralAsync(_workspace, managerUser, _client, _project, 4);

        var response = await PostRequestAsync(Url, managerJwt, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1
        });
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
        await _paymentSeeder.CreateSeveralAsync(_workspace, _user, _client, _project, 3);
        await _paymentSeeder.CreateSeveralAsync(_workspace, managerUser, _client, _project, 4);

        var response = await PostRequestAsync(Url, managerJwt, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1,
            MemberId = managerMember.Id
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(4, actualResponse.TotalCount);
        Assert.All(actualResponse.Items, item => Assert.Equal(managerUser.Id, item.Member.User.Id));
    }
}
