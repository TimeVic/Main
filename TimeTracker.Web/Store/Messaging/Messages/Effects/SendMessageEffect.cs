using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Messaging.Channels;

namespace TimeTracker.Web.Store.Messaging.Messages.Effects;

public class SendMessageEffect: Effect<SendMessageAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ChannelsState> _channelsState;
    private readonly ApiService _apiService;
    private readonly ILogger<SendMessageEffect> _logger;

    public SendMessageEffect(
        ApiService apiService,
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
