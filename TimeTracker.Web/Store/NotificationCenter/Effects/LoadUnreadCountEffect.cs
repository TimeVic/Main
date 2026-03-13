using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.NotificationCenter.Effects;

public class LoadUnreadCountEffect: Effect<LoadUnreadCountAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadUnreadCountEffect> _logger;

    public LoadUnreadCountEffect(
        ApiService apiService,
        IState<AuthState> authState,
        ILogger<LoadUnreadCountEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadUnreadCountAction action, IDispatcher dispatcher)
    {
        try
        {
            var count = await _apiService.NotificationCenterGetUnreadCount(_authState.Value.Workspace!.Id);
            dispatcher.Dispatch(new SetUnreadCountAction(count));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
