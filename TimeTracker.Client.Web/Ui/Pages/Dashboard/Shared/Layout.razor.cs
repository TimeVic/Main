using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Web.Services.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared;

public partial class Layout: IDisposable
{
    [Inject]
    private WorkspaceInitializationService _workspaceInitializationService { get; set; }
    
    [Inject]
    private NavigationManager _navigationManager { get; set; }

    [Inject]
    private UrlService _urlService { get; set; } = null!;

    private IDisposable? _locationChangingRegistration;
    
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = true;
        _locationChangingRegistration = _navigationManager.RegisterLocationChangingHandler(OnLocationChangingAsync);
        await base.OnInitializedAsync();
    }

    public new void Dispose()
    {
        _locationChangingRegistration?.Dispose();
        base.Dispose();
    }

    private async ValueTask OnLocationChangingAsync(LocationChangingContext context)
    {
        var workspaceId = _urlService.GetWorkspaceIdFromDashboardUrl(context.TargetLocation);
        if (!workspaceId.HasValue || workspaceId == AuthState.Value.Workspace?.Id)
        {
            return;
        }

        if (await _workspaceInitializationService.EnsureWorkspaceAsync(workspaceId))
        {
            _workspaceInitializationService.Init(isReload: true);
            await _workspaceInitializationService.AfterInit(isReload: true);
            return;
        }

        context.PreventNavigation();
        _navigationManager.NavigateTo(SiteUrl.Error403, replace: true);
    }
}
