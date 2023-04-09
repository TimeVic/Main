using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Services.Workspace;

public class WorkspaceInitializationService
{
    private readonly IDispatcher _dispatcher;
    private readonly NavigationManager _navigationManager;

    public WorkspaceInitializationService(
        IDispatcher dispatcher,
        NavigationManager navigationManager
    )
    {
        _dispatcher = dispatcher;
        _navigationManager = navigationManager;
    }

    public async Task Init(bool isReload = false)
    {
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Workspace.LoadListAction(isReload));
    }
    
    public async Task AfterInit(bool isReload = false)
    {
        _dispatcher.Dispatch(new TimeTracker.Web.Store.WorkspaceMemberships.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Project.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Client.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.LoadListAction(isReload));
        if (!_navigationManager.GetPath().Equals(SiteUrl.DashboardBase))
        {
            _dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction(0));
        }
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Tag.LoadListAction());
    }
}
