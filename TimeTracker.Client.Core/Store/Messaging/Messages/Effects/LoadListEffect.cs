using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Messaging.Channels;

namespace TimeTracker.Client.Core.Store.Messaging.Messages.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ChannelsState> _channelsState;
    private readonly IState<MessagesState> _messagesState;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<ChannelsState> channelsState,
        IState<MessagesState> messagesState,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _channelsState = channelsState;
        _messagesState = messagesState;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsListLoadingAction(true));
            if (action.IsRefresh)
            {
                dispatcher.Dispatch(new RefreshListAction(action.Channel));
            }

            var listState = _messagesState.Value.GetListState(action.Channel);
            var page = listState?.Page ?? 1;
            if (_authState.Value.Workspace != null)
            {
                if (_channelsState.Value.SelectedChannel == null)
                {
                    dispatcher.Dispatch(new RefreshListAction(action.Channel));
                    return;
                }
                if (page < 1)
                {
                    page = 1;
                    dispatcher.Dispatch(new RefreshListAction(action.Channel));
                }

                if (listState is not { IsListFullListLoaded: true })
                {
                    var response = await _apiService.MessagingMessageGetListAsync(
                        _channelsState.Value.SelectedChannel!.Id,
                        page
                    );
                    dispatcher.Dispatch(new SetListAction(action.Channel, response!));
                    
                    page++;
                    dispatcher.Dispatch(new SetPageAction(action.Channel, page));
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoadingAction(false));
        }
    }
}
