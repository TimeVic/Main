using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services.Http;

namespace TimeTracker.Web.Store.Auth.Effects;

public class LogoutEffect: Effect<LogoutAction>
{
    private readonly NavigationManager _navigationManager;
    private readonly ApiService _apiService;
    private readonly ILogger<LogoutEffect> _logger;

    public LogoutEffect(
        NavigationManager navigationManager,
        ApiService apiService,
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

        _navigationManager.NavigateTo(SiteUrl.Login, forceLoad: true);
    }
}
