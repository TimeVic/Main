using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants.Messaging;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;
using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Messaging.Channel;

public class InitTest: BaseTest
{
    private const string Hub = "messaging";
    private readonly string Url = "/messaging/channel/init";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private string _receiverJwtToken;
    private UserEntity _receiver;
    private WorkspaceEntity _receiverWorkspace;

    public InitTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _projectFactory = ServiceProvider.GetRequiredService<IDataFactory<ProjectEntity>>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        (_receiverJwtToken, _receiver, _receiverWorkspace) = UserSeeder.CreateAuthorizedAndShareAsync(_workspace).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new InitRequest()
        {
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldCreate()
    {
        // Arrange
        var connection = CreateWebSocketConnection(Hub, _jwtToken);
        MessagingChannelDto? createdEntity = null;
        connection.On<MessagingChannelDto>(HubMethodName.ChannelCreated, msg =>
        {
            createdEntity = msg;
        });
        await connection.StartAsync();
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new InitRequest()
        {
            WorkspaceId = _workspace.Id
        });
        await response.EnsureSuccessStatusCodeWithoutError();

        // Assert
        Thread.Sleep(50);
        Assert.NotNull(createdEntity);
        Assert.NotNull(createdEntity.CreatedBy);
        Assert.NotEqual(Guid.Empty, createdEntity.Id);
        Assert.Equal(MessagingChannelType.Direct, createdEntity.Type);
        Assert.Equal(_user.Id, createdEntity.CreatedBy.Id);
    }
}
