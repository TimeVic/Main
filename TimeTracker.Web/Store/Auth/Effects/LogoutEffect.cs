using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;

namespace TimeTracker.Web.Store.Auth.Effects;

public class LogoutEffect: Effect<LogoutAction>
{
    private readonly NavigationManager _navigationManager;

    public LogoutEffect(
        NavigationManager navigationManager
    )
    {
        _navigationManager = navigationManager;
    }

    public override Task HandleAsync(LogoutAction pageAction, IDispatcher dispatcher)
    {
        _navigationManager.NavigateTo(SiteUrl.Login, forceLoad: true);
        return Task.CompletedTask;
    }
}
