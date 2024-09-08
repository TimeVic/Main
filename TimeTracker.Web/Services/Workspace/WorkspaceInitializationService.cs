using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Services.Messaging;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Services.Workspace;

public class WorkspaceInitializationService
{
    private readonly IDispatcher _dispatcher;
    private readonly NavigationManager _navigationManager;
    private readonly FcmService _fcmService;
    private readonly IState<AuthState> _authState;

    public WorkspaceInitializationService(
        IDispatcher dispatcher,
        NavigationManager navigationManager,
        FcmService fcmService,
        IState<AuthState> authState
    )
    {
        _dispatcher = dispatcher;
        _navigationManager = navigationManager;
        _fcmService = fcmService;
        _authState = authState;
    }

    public void Init(bool isReload = false)
    {
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Workspace.LoadListAction(isReload));
    }
    
    public void AfterInit(bool isReload = false)
    {
        _dispatcher.Dispatch(new SetIsWorkspaceInitializedAction(false));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.WorkspaceMemberships.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Project.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Client.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.LoadListAction(isReload));
        if (!_navigationManager.GetPath().Equals(SiteUrl.DashboardBase))
        {
            _dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.SetSelectedPageAction(1));
            _dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction());
        }
        
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Tag.LoadListAction());
        _dispatcher.Dispatch(new SetIsWorkspaceInitializedAction(true));
        Task.Run(() => _fcmService.SetNotificationToken());
    }
    
    public void ChangeWorkspace(WorkspaceDto workspace)
    {
        if (workspace.Id == _authState.Value.Workspace?.Id)
        {
            return;
        }
        _dispatcher.Dispatch(new SetWorkspaceAction(workspace));
        _dispatcher.Dispatch(new PersistDataAction());
        _navigationManager.ReloadPage();
    }
}
