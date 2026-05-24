using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Web.Services.Notification;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Common;
using TimeTracker.Client.Core.Store.Permissions;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Web.Services.Workspace;

public class WorkspaceInitializationService
{
    private readonly IDispatcher _dispatcher;
    private readonly NavigationManager _navigationManager;
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<WorkspaceInitializationService> _logger;

    public WorkspaceInitializationService(
        IDispatcher dispatcher,
        NavigationManager navigationManager,
        FcmService fcmService,
        IState<AuthState> authState,
        ApiService apiService,
        ILogger<WorkspaceInitializationService> logger
    )
    {
        _dispatcher = dispatcher;
        _navigationManager = navigationManager;
        _authState = authState;
        _apiService = apiService;
        _logger = logger;
    }

    public void Init(bool isReload = false)
    {
        _dispatcher.Dispatch(new LoadListAction(isReload));
    }
    
    public async Task AfterInit(bool isReload = false)
    {
        _dispatcher.Dispatch(new SetIsWorkspaceInitializedAction(false));
        _dispatcher.Dispatch(new LoadCurrentUserAction());
        await LoadWorkspacePermissionsAsync();
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.WorkspaceMembers.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Client.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.LoadListAction(isReload));
        if (!_navigationManager.GetPath().Equals(SiteUrl.DashboardBase))
        {
            _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TimeEntry.SetSelectedPageAction(1));
            _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TimeEntry.LoadListAction());
        }
        
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tag.LoadListAction());
        _dispatcher.Dispatch(new SetIsWorkspaceInitializedAction(true));
        // Task.Run(() => _fcmService.SetNotificationToken());
    }

    private async Task LoadWorkspacePermissionsAsync()
    {
        var workspaceId = _authState.Value.Workspace?.Id;
        if (workspaceId == null)
        {
            _dispatcher.Dispatch(new ClearWorkspacePermissionsAction());
            return;
        }

        try
        {
            _dispatcher.Dispatch(new SetWorkspacePermissionsLoadingAction(true));
            var response = await _apiService.GetWorkspacePermissionsAsync(workspaceId.Value);
            if (response != null)
            {
                _dispatcher.Dispatch(new SetWorkspacePermissionsAction(response));
            }
            else
            {
                _dispatcher.Dispatch(
                    new SetWorkspacePermissionsAction(
                        new GetWorkspacePermissionsResponse
                        {
                            Permissions = new List<WorkspacePermission>()
                        }
                    )
                );
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            _dispatcher.Dispatch(
                new SetWorkspacePermissionsAction(
                    new GetWorkspacePermissionsResponse
                    {
                        Permissions = new List<WorkspacePermission>()
                    }
                )
            );
        }
        finally
        {
            _dispatcher.Dispatch(new SetWorkspacePermissionsLoadingAction(false));
        }
    }
    
    public void ChangeWorkspace(WorkspaceDto workspace)
    {
        if (workspace.Id == _authState.Value.Workspace?.Id)
        {
            return;
        }
        _dispatcher.Dispatch(new SelectWorkspaceAction(workspace));
    }
}
