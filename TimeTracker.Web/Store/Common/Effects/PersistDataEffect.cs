using Blazored.LocalStorage;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Common.Effects;

public class PersistDataAEffect: AEffectPersistData<PersistDataAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<PersistDataAEffect> _logger;

    public PersistDataAEffect(
        IState<AuthState> authState,
        ILocalStorageService localStorage,
        NavigationManager navigationManager,
        ILogger<PersistDataAEffect> logger
    )
    {
        _authState = authState;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _logger = logger;
    }

    public override async Task HandleAsync(PersistDataAction pageAction, IDispatcher dispatcher)
    {
        _logger.LogDebug("Persist data to local storage");
        if (_authState.Value.IsLoggedIn)
        {
            await SetData(AuthDataKey, _authState.Value);
        }
        else
        {
            await _localStorage.RemoveItemAsync(AuthDataKey);
        }

        if (pageAction.RedirectToLoginAfterPersist)
        {
            _navigationManager.NavigateTo(SiteUrl.Login, forceLoad: true);
        }
    }

    private async Task SetData(string key, object data)
    {
        await _localStorage.SetItemAsStringAsync(
            key,
            JsonConvert.SerializeObject(data)
        );
    }
}
