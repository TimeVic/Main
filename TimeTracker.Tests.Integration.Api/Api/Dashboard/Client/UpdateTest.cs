using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Client;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/client/update";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;
    private readonly IClientSeeder _clientSeeder;
    private readonly ClientEntity _client;
    private readonly IDataFactory<ClientEntity> _factory;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        _factory = ServiceProvider.GetRequiredService<IDataFactory<ClientEntity>>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        
        _client = _clientSeeder.Create(_user.DefaultWorkspace).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            Name = _client.Name,
            Id = _client.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        var client = _factory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            Name = client.Name,
            Id = _client.Id
        });
        await response.EnsureSuccessStatusCodeWithoutError();

        var actualProject = await response.GetJsonDataAsync<ClientDto>();
        Assert.NotEqual(Guid.Empty, actualProject.Id);
        Assert.Equal(client.Name, actualProject.Name);
    }
}
