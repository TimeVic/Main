using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Payment;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/payment/list";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<PaymentEntity> _factory;
    private readonly string _jwtToken;
    private readonly IClientDao _clientDao;
    private readonly WorkspaceEntity _workspace;
    private readonly ClientEntity _client;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly IPaymentSeeder _paymentSeeder;
    private readonly IUserSeeder _userSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<PaymentEntity>>();
        _clientDao = ServiceProvider.GetRequiredService<IClientDao>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IPaymentSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _client = _clientDao.CreateAsync(_workspace, "Test new client").Result;
        _project = _projectDao.CreateAsync(_workspace, "Test new project").Result;
        _project.SetClient(_client);
        DbSessionProvider.PerformCommitAsync().Wait();
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
            Assert.True(item.Id > 0);
            Assert.Equal(_client.Id, item.Client.Id);
            Assert.Equal(_project.Id, item.Project.Id);
            Assert.True(item.Amount > 0);
            Assert.NotEmpty(item.Description);
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
        var errorResponse = await response.GetJsonErrorAsync();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), errorResponse.Type);
    }
    
    [Fact]
    public async Task ShouldReceiveOnlyForCurrentUser()
    {
        var (otherJwt, otherUser, otherWorkspace) = await _userSeeder.CreateAuthorizedAsync();
        var otherClient = _clientDao.CreateAsync(otherWorkspace, "Test new client").Result;
        var otherProject = _projectDao.CreateAsync(otherWorkspace, "Test new project").Result;
        otherProject.SetClient(otherClient);
        await _paymentSeeder.CreateSeveralAsync(otherWorkspace, otherUser, otherClient, otherProject, 5);
        
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
    }
}
