using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Messaging.Channels;

namespace TimeTracker.Client.Core.Store.Messaging.Messages.Effects;

public class SendMessageEffect: Effect<SendMessageAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ChannelsState> _channelsState;
    private readonly IApiService _apiService;
    private readonly ILogger<SendMessageEffect> _logger;

    public SendMessageEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<ChannelsState> channelsState,
        ILogger<SendMessageEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _channelsState = channelsState;
        _logger = logger;
    }

    public override async Task HandleAsync(SendMessageAction action, IDispatcher dispatcher)
    {
        try
        {
            var selectedChannel = _channelsState.Value.SelectedChannel;
            if (selectedChannel == null)
            {
                return;
            }

            dispatcher.Dispatch(new SetIsMessageSending(true));
            if (_authState.Value.Workspace != null)
            {
                await _apiService.MessagingMessageSendAsync(
                    _authState.Value.Workspace.Id,
                    action.Text,
                    receiverId: null,
                    channelId: selectedChannel.Id
                );
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsMessageSending(false));
        }
    }
}
