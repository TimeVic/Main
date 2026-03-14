using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.WebSocket.Constants;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.WebSocket.MessagingHub
{
    public class SendMessageTests : BaseTest
    {
        private readonly IUserSeeder _userSeeder;

        public SendMessageTests(ApiCustomWebApplicationFactory factory) : base(factory)
        {
            _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        }

        private const string Hub = "messaging";
        private const string MethodName = "SendMessage";
        
        [Fact]
        public async Task ShouldNotSendIfUnauthorized()
        {
            // Arrange
            var sender = await _userSeeder.CreateActivatedAsync();
            var workspace = sender.CreatedWorkspaces.First();
            var receiver = await _userSeeder.CreateActivatedAndShareAsync(sender.CreatedWorkspaces.First(), MembershipAccessType.User);
            var connection = CreateWebSocketConnection(Hub);
            await connection.StartAsync();
            
            // Act
            // Assert
            await FlushDbChanges();
            await Assert.ThrowsAsync<HubException>(async () =>
            {
                await connection.InvokeAsync(
                    MethodName,
                    workspace.Id.ToString(),
                    "Test messgage",
                    receiver.Id,
                    null
                );
            });
        }
        
        [Fact]
        public async Task ShouldSendDirectMessage()
        {
            // Arrange
            var (senderJwtToken, sender, defaultWorkspace) = await _userSeeder.CreateAuthorizedAsync();
            var receiver = await _userSeeder.CreateActivatedAndShareAsync(defaultWorkspace, MembershipAccessType.User);
            var connection = CreateWebSocketConnection(Hub, senderJwtToken);
            
            MessagingMessageDto? createdMessage = null;
            connection.On<MessagingMessageDto>(HubMethodName.MessageCreated, msg =>
            {
                createdMessage = msg;
            });
            await connection.StartAsync();
            
            // Act
            await FlushDbChanges();
            await connection.InvokeAsync(
                MethodName,
                defaultWorkspace.Id.ToString(),
                "Test messgage",
                receiver.Id,
                null
            );
            
            // Assert
            Thread.Sleep(50);
            Assert.NotNull(createdMessage);
            Assert.NotEqual(Guid.Empty, createdMessage.Id);
            Assert.Equal("Test messgage", createdMessage.Text);
            Assert.NotNull(createdMessage.CreatedBy);
            Assert.Equal(sender.Id, createdMessage.CreatedBy.Id);
            Assert.NotNull(createdMessage.Channel);
        }
    }
}
