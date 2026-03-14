using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Messaging;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Pages.Dashboard.Shared;

public partial class Layout
{
    [Inject]
    private WorkspaceInitializationService _workspaceInitializationService { get; set; }
    
    [Inject]
    private NavigationManager _navigationManager { get; set; }
    
    [Inject]
    private MessagingWebSocketClientService _webSocketClient { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = true;
        await base.OnInitializedAsync();
    }

    private async Task ConnecToWs()
    {
        await _webSocketClient.Connect();
    }

    private void Send()
    {
        _webSocketClient.Send();
    }
    
    private void Disconnect()
    {
        _webSocketClient.Dispose();
    }
}
