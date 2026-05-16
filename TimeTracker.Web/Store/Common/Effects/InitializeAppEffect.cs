using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Common.Effects;

public class InitializeAppEffect: Effect<InitializeAppAction>
{
    private readonly ILogger<InitializeAppEffect> _logger;
    private readonly IState<CommonState> _state;
    private readonly ApiService _apiService;

    public InitializeAppEffect(
        ILogger<InitializeAppEffect> logger,
        IState<CommonState> state,
        ApiService apiService
    )
    {
        _logger = logger;
        _state = state;
        _apiService = apiService;
    }

    public override async Task HandleAsync(InitializeAppAction pageAction, IDispatcher dispatcher)
    {
        if (_state.Value.IsInitialized)
        {
            _logger.LogDebug("App is already initialized, skip initialization");
            return;
        }

        try
        {
            _logger.LogDebug("Check current user session");
            var isLoggedIn = await _apiService.CheckIsLoggedInAsync();
            if (isLoggedIn)
            {
                var user = await _apiService.UserGetCurrentAsync();
                if (user != null)
                {
                    dispatcher.Dispatch(new UpdateUserAction(user));
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsAppInitializedAction(IsInitialized: true));
        }
    }
}
