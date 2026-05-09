using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Client;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/client/add";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private new readonly IDataFactory<ClientEntity> _factory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        _factory = ServiceProvider.GetRequiredService<IDataFactory<ClientEntity>>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var client = _factory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            Name = client.Name,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var client = _factory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Name = client.Name,
        });
        await response.EnsureSuccessStatusCodeWithoutError();

        var actualProject = await response.GetJsonDataAsync<ProjectDto>();
        Assert.NotEqual(Guid.Empty, actualProject.Id);
        Assert.Equal(client.Name, actualProject.Name);
    }
    
    [Fact]
    public async Task ShouldNotAddIfIncorrectWorkspaceId()
    {
        var user2 = await UserSeeder.CreateActivatedAsync();
        var client = _factory.Generate();
        var workspaces = await _userDao.GetUsersWorkspaces(user2, MembershipAccessType.Owner);
        
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            Name = client.Name,
        }, workspaces.First().Id);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseData = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), responseData.ErrorCode);
    }
}
