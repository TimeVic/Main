using Fluxor;
using TimeTracker.Client.Core.Services;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Common.Effects;

public class InitializeAppEffect: Effect<InitializeAppAction>
{
    private readonly ILogger<InitializeAppEffect> _logger;
    private readonly IState<CommonState> _state;
    private readonly IApiService _apiService;
    private readonly IUserLocaleService _userLocaleService;

    public InitializeAppEffect(
        ILogger<InitializeAppEffect> logger,
        IState<CommonState> state,
        IApiService apiService,
        IUserLocaleService userLocaleService
    )
    {
        _logger = logger;
        _state = state;
        _apiService = apiService;
        _userLocaleService = userLocaleService;
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
                    await _userLocaleService.ApplyUserLocaleAsync(user);
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
