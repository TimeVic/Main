using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Messaging.Message;

public class GetListTest: BaseTest
{
    private const string Hub = "messaging";
    private readonly string Url = "/messaging/message/get-list";
    
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
        var channel = await _messagingDao.CreateChannel(_workspace, _user, "test");
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            ChannelId = channel.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldGet()
    {
        // Arrange
        var channel = await _messagingDao.CreateChannel(_workspace, _user, "test2");
        await _messagingDao.CreateMessage(channel, _user, "test1");
        await _messagingDao.CreateMessage(channel, _user, "test2");
        await _messagingDao.CreateMessage(channel, _user, "test3");
        
        var channelNotExpected = await _messagingDao.CreateChannel(_workspace, _user, "test");
        await _messagingDao.CreateMessage(channelNotExpected, _user, "test1");
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1,
            ChannelId = channel.Id
        });
        await response.EnsureSuccessStatusCodeWithoutError();

        // Assert
        var responseData = await response.GetJsonDataAsync<GetListResponse>();
        
        Assert.NotEmpty(responseData.Items);
        Assert.Equal(3, responseData.Items.Count);
        Assert.Contains(responseData.Items, item => item.Channel.Id == channel.Id);
    }
    
    [Fact]
    public async Task ShouldGetSecondPage()
    {
        // Arrange
        var channel = await _messagingDao.CreateChannel(_workspace, _user, "test2");
        for (int i = 0; i < 15; i++)
        {
            await _messagingDao.CreateMessage(channel, _user, $"test{i}");    
        }
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 2,
            ChannelId = channel.Id
        });
        await response.EnsureSuccessStatusCodeWithoutError();

        // Assert
        var responseData = await response.GetJsonDataAsync<GetListResponse>();
        
        Assert.NotEmpty(responseData.Items);
        Assert.Equal(5, responseData.Items.Count);
        Assert.Contains(responseData.Items, item => item.Channel.Id == channel.Id);
    }
}
