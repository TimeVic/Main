using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Messaging;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Messaging.Messages;

namespace TimeTracker.Web.Ui.Pages.Chat.Parts.Messages;

public partial class MessagesListBlock: IDisposable
{
    [Inject]
    protected IState<MessagesState> State { get; set; }
    
    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    [Inject] 
    private MessagingWebSocketClientService _webSocketClientService { get; set; }

    [Inject]
    protected UiHelperService UiHelperService { get; set; }
    
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }
    
    private readonly Subject<ICollection<MessagingMessageDto>> _messagesSubject = new();
    private ICollection<MessagingMessageDto> _messages = new List<MessagingMessageDto>();
    
    protected override async Task OnInitializedAsync()
    {
        _webSocketClientService.OnMessageCreated += OnMessageCreated;
        
        await base.OnInitializedAsync();
        
        _messagesSubject
            .Select(items =>
            {
                return items
                    .OrderByDescending(item => item.CreatedAt)
                    .ToArray();
            })
            .Subscribe(async results =>
            {
                _messages = results;
                StateHasChanged();
                if (State.Value.Page == 1)
                {
                    await UiHelperService.ScrollToBottom("tv-chat-container");
                }
            });
        
        RunAfterRendered(async () =>
        {
            await UiHelperService.OnScrollTopReached(
                "tv-chat-container",
                DotNetObjectReference.Create(this)
            );
        });
        
        ActionSubscriber.SubscribeToAction<TimeTracker.Web.Store.Messaging.Messages.SetListAction>(this, async (action) =>
        {
            _messagesSubject.OnNext(State.Value.List);
            StateHasChanged();
        });
    }
    
    [JSInvokable]
    public Task OnScrollTopReached()
    {
        Dispatcher.Dispatch(new LoadListAction(false));
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _webSocketClientService.OnMessageCreated -= OnMessageCreated;
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _messagesSubject.Dispose();
    }
    
    private void OnMessageCreated(MessagingMessageDto message)
    {
        Dispatcher.Dispatch(new AddMessageAction(message));
    }
}
