using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimeTracker.Api.WebSocket.Constants;
using TimeTracker.Api.WebSocket.Core;
using IAuthorizationService = TimeTracker.Business.Services.Auth.IAuthorizationService;

namespace TimeTracker.Api.WebSocket.Hubs.PingHub;

public class PingHub : BaseHub
{
    public PingHub(ILifetimeScope scope): base(scope)
    {
    }
    
    public async Task Ping()
    {
        await ExecuteInScopeAsync(async sp =>
        {
            // Call the broadcastMessage method to update clients.
            await Clients.Caller.SendAsync(HubMethodName.PongResponse, "Pong");
        });
    }
    
    public async Task PingWithMessage(string message)
    {
        await ExecuteInScopeAsync(async sp =>
        {
            // Call the broadcastMessage method to update clients.
            await Clients.Caller.SendAsync(HubMethodName.PongResponse, $"Pong: {message}");
        });
    }
    
    [Authorize]
    public async Task PingWithAuth()
    {
        await ExecuteInScopeAsync(async sp =>
        {
            var authService = sp.GetRequiredService<IAuthorizationService>();
            var user = await authService.GetCurrentLoggedInUser();
        
            ArgumentNullException.ThrowIfNull(user);
            // Call the broadcastMessage method to update clients.
            await Clients.Caller.SendAsync(HubMethodName.PongResponse, $"Pong {user.Email}");
        });
    }
}
