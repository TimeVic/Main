using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message;
using TimeTracker.Api.WebSocket.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace TimeTracker.Tests.Integration.Api.Api.Messaging.Message;

public class SendTest: BaseTest
{
    private const string Hub = "messaging";
    private readonly string Url = "/messaging/message/send";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private string _receiverJwtToken;
    private UserEntity _receiver;
    private WorkspaceEntity _receiverWorkspace;

    public SendTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _projectFactory = ServiceProvider.GetRequiredService<IDataFactory<ProjectEntity>>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        (_receiverJwtToken, _receiver, _receiverWorkspace) = UserSeeder.CreateAuthorizedAndShareAsync(_workspace).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new SendRequest()
        {
            Text = "Some text",
            WorkspaceId = _workspace.Id,
            ReceiverId = _receiver.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldSend()
    {
        // Arrange
        var expectedMessage = "some messaGe";
        var connection = CreateWebSocketConnection(Hub, _jwtToken);
        MessagingMessageDto? createdMessage = null;
        connection.On<MessagingMessageDto>(HubMethodName.MessageCreated, msg =>
        {
            createdMessage = msg;
        });
        await connection.StartAsync();
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new SendRequest()
        {
            Text = expectedMessage,
            WorkspaceId = _workspace.Id,
            ReceiverId = _receiver.Id
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        // Assert
        Thread.Sleep(50);
        Assert.NotNull(createdMessage);
        Assert.NotNull(createdMessage.CreatedBy);
        Assert.NotEqual(Guid.Empty, createdMessage.Id);
        Assert.Equal(expectedMessage, createdMessage.Text);
        Assert.Equal(_user.Id, createdMessage.CreatedBy.Id);
    }
}
