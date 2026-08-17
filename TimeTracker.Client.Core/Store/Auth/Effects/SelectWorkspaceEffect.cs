using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Permissions;

namespace TimeTracker.Client.Core.Store.Auth.Effects;

public class SelectWorkspaceEffect : Effect<SelectWorkspaceAction>
{
    private readonly IApiService _apiService;
    private readonly NavigationManager _navigationManager;
    private readonly UrlService _urlService;
    private readonly ILogger<SelectWorkspaceEffect> _logger;

    public SelectWorkspaceEffect(
        IApiService apiService,
        NavigationManager navigationManager,
        UrlService urlService,
        ILogger<SelectWorkspaceEffect> logger
    )
    {
        _apiService = apiService;
        _navigationManager = navigationManager;
        _urlService = urlService;
        _logger = logger;
    }

    public override async Task HandleAsync(SelectWorkspaceAction action, IDispatcher dispatcher)
    {
        var selectedWorkspace = action.Workspace;

        try
        {
            var user = await _apiService.UserSelectWorkspaceAsync(action.Workspace.Id);
            if (user != null)
            {
                dispatcher.Dispatch(new UpdateUserAction(user));
                selectedWorkspace = user.SelectedWorkspace ?? action.Workspace;
                dispatcher.Dispatch(new SetWorkspaceAction(selectedWorkspace));
            }
            else
            {
                dispatcher.Dispatch(new SetWorkspaceAction(selectedWorkspace));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            dispatcher.Dispatch(new SetWorkspaceAction(selectedWorkspace));
        }

        dispatcher.Dispatch(new ReloadWorkspacePermissionsAction());
        var destinationPath = action.DestinationPath
            ?? (selectedWorkspace.Mode.HasValue ? string.Empty : "workspace/choose-mode");
        _navigationManager.NavigateTo(
            _urlService.GetDashboardUrl(destinationPath, selectedWorkspace.Id),
            forceLoad: true
        );
    }
}
