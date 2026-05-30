using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Services.Messaging;
using TimeTracker.Client.Web.Services.UI;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Messaging.Channels;
using TimeTracker.Client.Core.Store.Messaging.Messages;
using LoadListAction = TimeTracker.Client.Core.Store.Messaging.Messages.LoadListAction;

namespace TimeTracker.Client.Web.Ui.Pages.Chat.Parts.Messages;

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
    private IEnumerable<MessagingMessageDto> _sortedMessages => _messages.OrderBy(item => item.CreatedAt).AsQueryable();
    private bool _isNeedsScrollToBottom;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);        
        if (_isNeedsScrollToBottom)
        {
            _isNeedsScrollToBottom = false;
            await UiHelperService.ScrollToBottom("tv-chat-container");
        }
    }

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
                foreach (var messagingMessageDto in results)
                {
                    if (_messages.Contains(messagingMessageDto))    
                        continue;
                    _messages.Add(messagingMessageDto);
                }
                StateHasChanged();
                var listState = GetListState();
                if (listState != null)
                {
                    if (listState.Page == 1)
                    {
                        _isNeedsScrollToBottom = true;
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
        
        ActionSubscriber.SubscribeToAction<TimeTracker.Client.Core.Store.Messaging.Messages.SetListAction>(this, async (action) =>
        {
            var listState = GetListState();
            if (listState != null)
            {
                _messagesSubject.OnNext(listState.List);    
            }
            StateHasChanged();
            if (!await UiHelperService.HasScroll("tv-chat-container"))
            {
                LoadList();
            }
        });
        ActionSubscriber.SubscribeToAction<TimeTracker.Client.Core.Store.Messaging.Channels.SetSelectedAction>(this, async (action) =>
        {
            LoadList(true);
            var listState = GetListState();
            if (listState != null)
            {
                _messages.Clear();
                _messagesSubject.OnNext(listState.List);    
            }
            _isNeedsScrollToBottom = true;
            StateHasChanged();
        });
        ActionSubscriber.SubscribeToAction<TimeTracker.Client.Core.Store.Messaging.Messages.AddMessageAction>(this, async (action) =>
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
