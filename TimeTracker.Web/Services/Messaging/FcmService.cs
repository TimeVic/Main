using Fluxor;
using Microsoft.JSInterop;
using MudBlazor;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Services.Messaging;

public class FcmService
{
    private readonly IJSRuntime _js;
    private readonly ApiService _apiService;
    private readonly IState<AuthState> _authState;
    private IJSObjectReference? _jsFcmModule;

    public FcmService(
        IJSRuntime js,
        ApiService apiService,
        IState<AuthState> authState
    )
    {
        _js = js;
        _apiService = apiService;
        _authState = authState;
    }

    public async Task SetNotificationToken()
    {
        if (!_authState.Value.IsLoggedIn)
        {
            return;
        }

        _jsFcmModule = await _js.InvokeAsync<IJSObjectReference>("import", "./js/messaging.js");
        var newToken = await _jsFcmModule.InvokeAsync<string>("getToken");
        if (!string.IsNullOrEmpty(newToken))
        {
            await _apiService.SendNotificationToken(newToken);
        }
    }
}
