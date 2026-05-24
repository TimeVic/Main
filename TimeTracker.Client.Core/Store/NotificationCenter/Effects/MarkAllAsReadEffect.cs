using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.NotificationCenter.Effects;

public class MarkAllAsReadEffect: Effect<MarkAllAsReadAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly ILogger<MarkAllAsReadEffect> _logger;

    public MarkAllAsReadEffect(
        IApiService apiService,
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
