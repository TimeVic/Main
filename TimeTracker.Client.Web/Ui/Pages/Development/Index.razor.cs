using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.Messaging;

namespace TimeTracker.Client.Web.Ui.Pages.Development;

public partial class Index
{
    [Inject] 
    private MessagingWebSocketClientService _webSocketClientService { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        // IsRedirectIfNotLoggedIn = false;
        await base.OnInitializedAsync();
        await _webSocketClientService.Connect();
    }
}
