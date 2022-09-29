using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Payment;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/payment/add";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<PaymentEntity> _factory;
    private readonly string _jwtToken;
    private readonly IClientDao _clientDao;
    private readonly WorkspaceEntity _workspace;
    private readonly ClientEntity _client;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<PaymentEntity>>();
        _clientDao = ServiceProvider.GetRequiredService<IClientDao>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;

        _workspace = _user.DefaultWorkspace;
        _client = _clientDao.CreateAsync(_workspace, "Test adding").Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        _project.SetClient(_client);
        DbSessionProvider.PerformCommitAsync().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var payment = _factory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            ClientId = _client.Id,
            Amount = payment.Amount,
            Description = payment.Description,
            PaymentTime = payment.PaymentTime,
            ProjectId = _project.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var payment = _factory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            ClientId = _client.Id,
            Amount = payment.Amount,
            Description = payment.Description,
            PaymentTime = payment.PaymentTime,
            ProjectId = _project.Id
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualPayment = await response.GetJsonDataAsync<PaymentDto>();
        Assert.True(actualPayment.Id > 0);
        Assert.Equal(_client.Id, actualPayment.Client.Id);
        Assert.Equal(_project.Id, actualPayment.Project.Id);
        Assert.Equal(payment.Amount, actualPayment.Amount);
        Assert.Equal(payment.Description, actualPayment.Description);
        Assert.Equal(payment.PaymentTime, actualPayment.PaymentTime);
    }
}
