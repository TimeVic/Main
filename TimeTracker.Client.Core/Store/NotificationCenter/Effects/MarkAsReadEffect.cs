using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.NotificationCenter.Effects;

public class MarkAsReadEffect: Effect<MarkAsReadAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly ILogger<MarkAsReadEffect> _logger;

    public MarkAsReadEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<MarkAsReadEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(MarkAsReadAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.NotificationCenterMarkAsRead(action.NotificationId);
            dispatcher.Dispatch(new SetAsReadAction(action.NotificationId));
            dispatcher.Dispatch(new LoadUnreadCountAction());
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
