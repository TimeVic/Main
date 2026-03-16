using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Services.Messaging;

namespace TimeTracker.Web.Ui.Pages.Development;

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
