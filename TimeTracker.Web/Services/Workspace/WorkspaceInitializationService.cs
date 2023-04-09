using Fluxor;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Services.Workspace;

public class WorkspaceInitializationService
{
    private readonly IDispatcher _dispatcher;

    public WorkspaceInitializationService(
        IDispatcher dispatcher
    )
    {
        _dispatcher = dispatcher;
    }

    public async Task Init(bool isReload = false)
    {
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Workspace.LoadListAction(isReload));
    }
    
    public async Task AfterInit(bool isReload = false)
    {
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Workspace.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.WorkspaceMemberships.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Project.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Client.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.LoadListAction(isReload));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction(0));
        _dispatcher.Dispatch(new TimeTracker.Web.Store.Tag.LoadListAction());
    }
}
