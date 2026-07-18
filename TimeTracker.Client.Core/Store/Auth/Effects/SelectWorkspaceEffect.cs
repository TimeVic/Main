using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

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
        try
        {
            var user = await _apiService.UserSelectWorkspaceAsync(action.Workspace.Id);
            if (user != null)
            {
                dispatcher.Dispatch(new UpdateUserAction(user));
                dispatcher.Dispatch(new SetWorkspaceAction(user.SelectedWorkspace ?? action.Workspace));
            }
            else
            {
                dispatcher.Dispatch(new SetWorkspaceAction(action.Workspace));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            dispatcher.Dispatch(new SetWorkspaceAction(action.Workspace));
        }

        _navigationManager.NavigateTo(
            _urlService.GetDashboardUrl(workspaceId: action.Workspace.Id),
            forceLoad: true
        );
    }
}
