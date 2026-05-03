using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
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
    private readonly IClientDao _clientDao;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;
    private readonly IMemberPaymentSeeder _paymentSeeder;
    private readonly MemberPaymentEntity _payment;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<MemberPaymentEntity>>();
        _clientDao = ServiceProvider.GetRequiredService<IClientDao>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IMemberPaymentSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _payment = _paymentSeeder.CreateSeveralAsync(_user, 1).Result.First();
        FlushDbChanges().Wait();

        Assert.NotNull(_payment);
        Assert.NotNull(_payment.Project);
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var expectMemberPayment = _factory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            WorkspaceId = _workspace.Id,
            MemberPaymentId = _payment.Id,
            ClientId = _payment.Client.Id,
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
        var expectedClient = await _clientDao.CreateAsync(_workspace, "Test new client");
        var expectProject = await _projectDao.CreateAsync(_workspace, "Test new project");
        expectProject.SetClient(expectedClient);
        await FlushDbChanges();
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            WorkspaceId = _workspace.Id,
            MemberPaymentId = _payment.Id,
            ClientId = expectedClient.Id,
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
        Assert.Equal(expectedClient.Id, actualMemberPayment.Client.Id);
        Assert.Equal(expectProject.Id, actualMemberPayment.Project.Id);
        Assert.Equal(expectMemberPayment.Amount, actualMemberPayment.Amount);
        Assert.Equal(expectMemberPayment.Description, actualMemberPayment.Description);
        Assert.Equal(expectMemberPayment.PaymentTime, actualMemberPayment.PaymentTime.ToUniversalTime());
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfHasNoAccess()
    {
        var expectMemberPayment = _factory.Generate();
        var (otherJwtToken, otherUser, otherWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        
        var response = await PostRequestAsync(Url, otherJwtToken, new UpdateRequest()
        {
            WorkspaceId = _workspace.Id,
            MemberPaymentId = _payment.Id,
            ClientId = _payment.Client.Id,
            Amount = expectMemberPayment.Amount,
            Description = expectMemberPayment.Description,
            PaymentTime = expectMemberPayment.PaymentTime,
            ProjectId = _payment.Project!.Id
        });
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
    }
}
