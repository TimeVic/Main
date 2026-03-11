using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.NotificationCenter.Effects;

public class MarkAllAsReadEffect: Effect<MarkAllAsReadAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<MarkAllAsReadEffect> _logger;

    public MarkAllAsReadEffect(
        ApiService apiService,
        IState<AuthState> authState,
        ILogger<MarkAllAsReadEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(MarkAllAsReadAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetAllAsReadAction());
            await _apiService.NotificationCenterMarkAllAsRead(_authState.Value.Workspace!.Id);
            dispatcher.Dispatch(new LoadUnreadCountAction());
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
