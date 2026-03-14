using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;
using TimeTracker.Api.WebSocket.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Messaging.Channel;

public class GetListTest: BaseTest
{
    private const string Hub = "messaging";
    private readonly string Url = "/messaging/channel/get-list";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private string _receiverJwtToken;
    private UserEntity _receiver;
    private WorkspaceEntity _receiverWorkspace;
    private readonly IMessagingDao _messagingDao;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _projectFactory = ServiceProvider.GetRequiredService<IDataFactory<ProjectEntity>>();
        _messagingDao = ServiceProvider.GetRequiredService<IMessagingDao>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        (_receiverJwtToken, _receiver, _receiverWorkspace) = UserSeeder.CreateAuthorizedAndShareAsync(_workspace).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldGet()
    {
        // Arrange
        var expected1 = await _messagingDao.CreateChannel(_workspace, _user, "test");
        var expected2 = await _messagingDao.CreateChannel(_workspace, _user, "test2");
        var noExpected = await _messagingDao.CreateChannel(_workspace, _user, "test3");
        foreach (var member in noExpected.Members)
        {
            member.DeactivatedAt = DateTime.UtcNow;
        }
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _workspace.Id
        });
        await response.EnsureSuccessStatusCodeWithoutError();

        // Assert
        var responseData = await response.GetJsonDataAsync<GetListResponse>();
        
        Assert.NotEmpty(responseData.Items);
        Assert.Equal(2, responseData.Items.Count);
        Assert.Contains(responseData.Items, item => item.Id == expected1.Id);
        Assert.Contains(responseData.Items, item => item.Id == expected2.Id);
    }
}
