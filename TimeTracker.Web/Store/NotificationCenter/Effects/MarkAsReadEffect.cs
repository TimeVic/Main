using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.NotificationCenter.Effects;

public class MarkAsReadEffect: Effect<MarkAsReadAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<MarkAsReadEffect> _logger;

    public MarkAsReadEffect(
        ApiService apiService,
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
