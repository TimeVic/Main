using Blazored.LocalStorage;
using Fluxor;
using Newtonsoft.Json;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Common.Effects;

public class LoadPersistedDataAEffect: AEffectPersistData<LoadPersistedDataAction>
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<LoadPersistedDataAEffect> _logger;
    private readonly IState<CommonState> _state;

    public LoadPersistedDataAEffect(
        ILocalStorageService localStorage,
        ILogger<LoadPersistedDataAEffect> logger,
        IState<CommonState> state
    )
    {
        _localStorage = localStorage;
        _logger = logger;
        _state = state;
    }

    public override async Task HandleAsync(LoadPersistedDataAction pageAction, IDispatcher dispatcher)
    {
        if (_state.Value.IsInitialized)
        {
            Debug.Log("App is already initialized, skip loading persisted data");
            return;
        }

        _logger.LogDebug("Load persisted data from local storage");
        var authData = await GetData<AuthState>(AuthDataKey);
        if (authData != null)
        {
            dispatcher.Dispatch(new LoginAction(authData.User, authData.Workspace!));
        }
        dispatcher.Dispatch(new SetIsAppInitializedAction(IsInitialized: true));
    }

    private async Task<TState?> GetData<TState>(string key)
    {
        try
        {
            var authDataString = await _localStorage.GetItemAsStringAsync(key);
            if (string.IsNullOrEmpty(authDataString))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<TState>(authDataString);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }

        return default;
    }
}
