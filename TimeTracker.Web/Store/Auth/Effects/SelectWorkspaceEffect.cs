using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Services.Http;

namespace TimeTracker.Web.Store.Auth.Effects;

public class SelectWorkspaceEffect : Effect<SelectWorkspaceAction>
{
    private readonly ApiService _apiService;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<SelectWorkspaceEffect> _logger;

    public SelectWorkspaceEffect(
        ApiService apiService,
        NavigationManager navigationManager,
        ILogger<SelectWorkspaceEffect> logger
    )
    {
        _apiService = apiService;
        _navigationManager = navigationManager;
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

        _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
    }
}
