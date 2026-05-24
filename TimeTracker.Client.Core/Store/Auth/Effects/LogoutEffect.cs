using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.Auth.Effects;

public class LogoutEffect: Effect<LogoutAction>
{
    private readonly NavigationManager _navigationManager;
    private readonly IApiService _apiService;
    private readonly ILogger<LogoutEffect> _logger;

    public LogoutEffect(
        NavigationManager navigationManager,
        IApiService apiService,
        ILogger<LogoutEffect> logger
    )
    {
        _navigationManager = navigationManager;
        _apiService = apiService;
        _logger = logger;
    }

    public override async Task HandleAsync(LogoutAction pageAction, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.LogoutAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        _navigationManager.NavigateTo(ClientSiteUrl.Login, forceLoad: true);
    }
}
