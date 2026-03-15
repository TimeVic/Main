using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants.Messaging;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.WebSocket
{
    public class PingActionTests : BaseTest
    {
        private readonly IUserSeeder _userSeeder;

        public PingActionTests(ApiCustomWebApplicationFactory factory) : base(factory)
        {
            _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        }

        private const string Hub = "ping";
        
        [Fact]
        public async Task ShouldPingHub()
        {
            // Arrange
            var connection = CreateWebSocketConnection(Hub);
            
            var responseMessage = string.Empty;
            connection.On<string>(HubMethodName.PongResponse, msg =>
            {
                responseMessage = msg;
            });
            await connection.StartAsync();

            // Act
            await FlushDbChanges();
            await connection.InvokeAsync("Ping");

            // Assert
            Thread.Sleep(50);
            Assert.Equal("Pong", responseMessage);
        }
        
        [Fact]
        public async Task ShouldPingAuthorized()
        {
            // Arrange
            var (jwtToken, user, _) = await _userSeeder.CreateAuthorizedAsync();
            
            var connection = CreateWebSocketConnection(Hub, jwtToken);
            
            var responseMessage = string.Empty;
            connection.On<string>(HubMethodName.PongResponse, msg =>
            {
                responseMessage = msg;
            });
            await connection.StartAsync();

            // Act
            await FlushDbChanges();
            await connection.InvokeAsync("PingWithAuth");

            // Assert
            Thread.Sleep(50);
            Assert.StartsWith("Pong", responseMessage);
            Assert.Contains(user.Email, responseMessage);
        }
    }
}
