using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
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

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/member-payments/update";
    
    private readonly UserEntity _user;
    private new readonly IDataFactory<MemberPaymentEntity> _factory;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IMemberPaymentSeeder _paymentSeeder;
    private readonly MemberPaymentEntity _payment;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<MemberPaymentEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IMemberPaymentSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_workspace).Wait();

        _payment = _paymentSeeder.CreateSeveralAsync(_user, 1).Result.First();
        _payment.Member.Workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_payment.Member.Workspace).Wait();
        FlushDbChanges().Wait();

        Assert.NotNull(_payment);
        Assert.NotNull(_payment.Project);
    }

    [Fact]
    public async Task UserCanNotUpdatePaymentInSoloWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        _payment.Member.Workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var expectMemberPayment = _factory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberPaymentId = _payment.Id,
            Amount = expectMemberPayment.Amount,
            Description = expectMemberPayment.Description,
            PaymentTime = expectMemberPayment.PaymentTime,
            ProjectId = _payment.Project!.Id,
        });
        
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var expectMemberPayment = _factory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            MemberPaymentId = _payment.Id,
            Amount = expectMemberPayment.Amount,
            Description = expectMemberPayment.Description,
            PaymentTime = expectMemberPayment.PaymentTime,
            ProjectId = _payment.Project!.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        var expectMemberPayment = _factory.Generate();
        var expectProject = await _projectSeeder.CreateAsync(_workspace);
        var expectedClient = expectProject.Client;
        await FlushDbChanges();
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberPaymentId = _payment.Id,
            Amount = expectMemberPayment.Amount,
            Description = expectMemberPayment.Description,
            PaymentTime = expectMemberPayment.PaymentTime,
            ProjectId = expectProject.Id
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualMemberPayment = await response.GetJsonDataAsync<MemberPaymentDto>();
        Assert.NotNull(actualMemberPayment);
        Assert.NotNull(actualMemberPayment.Project);
        Assert.NotEqual(Guid.Empty, actualMemberPayment.Id);
        Assert.Equal(expectedClient.Id, actualMemberPayment.Client!.Id);
        Assert.Equal(expectProject.Id, actualMemberPayment.Project.Id);
        Assert.Equal(expectMemberPayment.Amount, actualMemberPayment.Amount);
        Assert.Equal(expectMemberPayment.Description, actualMemberPayment.Description);
        Assert.Equal(expectMemberPayment.PaymentTime, actualMemberPayment.PaymentTime.ToUniversalTime());
    }

    [Fact]
    public async Task ManagerCanUpdatePaymentMember()
    {
        var (managerToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );
        var memberUser = await UserSeeder.CreateActivatedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        await FlushDbChanges();
        var member = _workspace.Members.First(item => item.User.Id == memberUser.Id);
        var expectMemberPayment = _factory.Generate();

        var response = await PostRequestAsync(Url, managerToken, new UpdateRequest()
        {
            MemberPaymentId = _payment.Id,
            MemberId = member.Id,
            Amount = expectMemberPayment.Amount,
            Description = expectMemberPayment.Description,
            PaymentTime = expectMemberPayment.PaymentTime,
            ProjectId = _payment.Project!.Id
        });
        response.EnsureSuccessStatusCode();

        var actualMemberPayment = await response.GetJsonDataAsync<MemberPaymentDto>();
        Assert.Equal(member.Id, actualMemberPayment.Member.Id);
        Assert.Equal(memberUser.Id, actualMemberPayment.Member.User.Id);
    }

    [Fact]
    public async Task UserCanNotUpdatePaymentMember()
    {
        var (userToken, user, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        await FlushDbChanges();
        var payment = (await _paymentSeeder.CreateSeveralAsync(_workspace, user, 1)).First();
        var memberUser = await UserSeeder.CreateActivatedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        await FlushDbChanges();
        var member = _workspace.Members.First(item => item.User.Id == memberUser.Id);
        var expectMemberPayment = _factory.Generate();

        var response = await PostRequestAsync(Url, userToken, new UpdateRequest()
        {
            MemberPaymentId = payment.Id,
            MemberId = member.Id,
            Amount = expectMemberPayment.Amount,
            Description = expectMemberPayment.Description,
            PaymentTime = expectMemberPayment.PaymentTime,
            ProjectId = payment.Project!.Id
        });

        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfHasNoAccess()
    {
        var expectMemberPayment = _factory.Generate();
        var (otherJwtToken, otherUser, otherWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        
        var response = await PostRequestAsync(Url, otherJwtToken, new UpdateRequest()
        {
            MemberPaymentId = _payment.Id,
            Amount = expectMemberPayment.Amount,
            Description = expectMemberPayment.Description,
            PaymentTime = expectMemberPayment.PaymentTime,
            ProjectId = _payment.Project!.Id
        });
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
    }
}
