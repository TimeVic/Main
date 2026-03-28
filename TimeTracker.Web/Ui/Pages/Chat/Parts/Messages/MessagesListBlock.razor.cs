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
using TimeTracker.Web.Store.Messaging.Channels;
using TimeTracker.Web.Store.Messaging.Messages;
using LoadListAction = TimeTracker.Web.Store.Messaging.Messages.LoadListAction;

namespace TimeTracker.Web.Ui.Pages.Chat.Parts.Messages;

public partial class MessagesListBlock: IDisposable
{
    [Inject]
    protected IState<MessagesState> State { get; set; }
    
    [Inject]
    protected IState<ChannelsState> ChannelsState { get; set; }
    
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
                    .OrderBy(item => item.CreatedAt)
                    .ToArray();
            })
            .Subscribe(async results =>
            {
                _messages = results;
                StateHasChanged();
                var listState = GetListState();
                if (listState != null)
                {
                    if (listState.Page == 1)
                    {
                        await UiHelperService.ScrollToBottom("tv-chat-container");
                    }
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
            var listState = GetListState();
            if (listState != null)
            {
                _messagesSubject.OnNext(listState.List);    
            }
            StateHasChanged();
        });
        ActionSubscriber.SubscribeToAction<TimeTracker.Web.Store.Messaging.Channels.SetSelectedAction>(this, async (action) =>
        {
            LoadList(true);
            var listState = GetListState();
            if (listState != null)
            {
                _messagesSubject.OnNext(listState.List);    
            }
            StateHasChanged();
        });
        ActionSubscriber.SubscribeToAction<TimeTracker.Web.Store.Messaging.Messages.AddMessageAction>(this, async (action) =>
        {
            var listState = GetListState();
            if (listState != null)
            {
                _messagesSubject.OnNext(listState.List);    
            }
            StateHasChanged();
            await UiHelperService.ScrollToBottom("tv-chat-container");
        });
    }
    
    [JSInvokable]
    public Task OnScrollTopReached()
    {
        LoadList();
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _webSocketClientService.OnMessageCreated -= OnMessageCreated;
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _messagesSubject.Dispose();
    }
    
    private void LoadList(bool isRefresh = false)
    {
        Dispatcher.Dispatch(new LoadListAction(ChannelsState.Value.SelectedChannel!, false));
    }
    
    private void OnMessageCreated(MessagingMessageDto message)
    {
        Debug.Log("Message created", message.Id);
        Dispatcher.Dispatch(new AddMessageAction(message));
    }
    
    private string GetMessageDayLabel(DateTime sentAt)
    {
        var messageDay = sentAt.Date;

        if (messageDay == DateTime.Today)
        {
            return "Today";
        }

        if (messageDay == DateTime.Today.AddDays(-1))
        {
            return "Yesterday";
        }
        return sentAt.ToString("dd MMM yyyy");
    }
    
    private MessagesListState? GetListState()
    {
        var channel = ChannelsState.Value.SelectedChannel;
        if (channel == null)
        {
            return null;
        }
        return State.Value.GetListState(channel);
    }
}
