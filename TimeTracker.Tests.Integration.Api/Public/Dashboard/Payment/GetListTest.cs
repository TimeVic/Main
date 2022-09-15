using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Payment;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/payment/update";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<PaymentEntity> _factory;
    private readonly string _jwtToken;
    private readonly IClientDao _clientDao;
    private readonly WorkspaceEntity _workspace;
    private readonly ClientEntity _client;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly IPaymentSeeder _paymentSeeder;
    private readonly PaymentEntity _payment;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<PaymentEntity>>();
        _clientDao = ServiceProvider.GetRequiredService<IClientDao>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IPaymentSeeder>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;

        _workspace = _user.DefaultWorkspace;
        _payment = _paymentSeeder.CreateSeveralAsync(_user, 1).Result.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var expectPayment = _factory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            PaymentId = _payment.Id,
            ClientId = _payment.Client.Id,
            Amount = expectPayment.Amount,
            Description = expectPayment.Description,
            PaymentTime = expectPayment.PaymentTime,
            ProjectId = _payment.Project.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        var expectPayment = _factory.Generate();
        var expectedClient = await _clientDao.CreateAsync(_workspace, "Test new client");
        var expectProject = await _projectDao.CreateAsync(_workspace, "Test new project");
        expectProject.SetClient(expectedClient);
        await DbSessionProvider.PerformCommitAsync();
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            PaymentId = _payment.Id,
            ClientId = expectedClient.Id,
            Amount = expectPayment.Amount,
            Description = expectPayment.Description,
            PaymentTime = expectPayment.PaymentTime,
            ProjectId = expectProject.Id
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualPayment = await response.GetJsonDataAsync<PaymentDto>();
        Assert.True(actualPayment.Id > 0);
        Assert.Equal(expectedClient.Id, actualPayment.Client.Id);
        Assert.Equal(expectProject.Id, actualPayment.Project.Id);
        Assert.Equal(expectPayment.Amount, actualPayment.Amount);
        Assert.Equal(expectPayment.Description, actualPayment.Description);
        Assert.Equal(expectPayment.PaymentTime, actualPayment.PaymentTime);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfHasNoAccess()
    {
        var expectPayment = _factory.Generate();
        var (otherJwtToken, otherUser) = UserSeeder.CreateAuthorizedAsync().Result;
        
        var response = await PostRequestAsync(Url, otherJwtToken, new UpdateRequest()
        {
            PaymentId = _payment.Id,
            ClientId = _payment.Client.Id,
            Amount = expectPayment.Amount,
            Description = expectPayment.Description,
            PaymentTime = expectPayment.PaymentTime,
            ProjectId = _payment.Project.Id
        });
        var errorResponse = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.Type);
    }
}
