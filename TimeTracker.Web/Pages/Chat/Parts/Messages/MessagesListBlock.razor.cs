using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Web.Services.Messaging;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Messaging.Messages;

namespace TimeTracker.Web.Pages.Chat.Parts.Messages;

public partial class MessagesListBlock: IDisposable
{
    [Inject]
    protected IState<MessagesState> State { get; set; }
    
    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    [Inject] 
    private MessagingWebSocketClientService _webSocketClientService { get; set; }

    protected override Task OnInitializedAsync()
    {
        _webSocketClientService.OnMessageCreated += OnMessageCreated;
        
        return base.OnInitializedAsync();
    }

    public void Dispose()
    {
        _webSocketClientService.OnMessageCreated -= OnMessageCreated;
    }
    
    private void OnMessageCreated(MessagingMessageDto message)
    {
        Dispatcher.Dispatch(new AddMessageAction(message));
    }
}
