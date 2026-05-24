using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.NotificationCenter.Effects;

public class LoadUnreadCountEffect: Effect<LoadUnreadCountAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadUnreadCountEffect> _logger;

    public LoadUnreadCountEffect(
        IApiService apiService,
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
