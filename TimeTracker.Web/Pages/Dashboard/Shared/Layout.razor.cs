
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Pages.Dashboard.Shared;

public partial class Layout
{
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = true;
        await base.OnInitializedAsync();
        if (AuthState.Value.IsLoggedIn)
        {
            await OnLoggedInAsync();
        }
    }
    
    protected override async Task OnLoggedInAsync()
    {
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Workspace.LoadListAction(false));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.WorkspaceMemberships.LoadListAction(false));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Project.LoadProjectListAction(false));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Client.LoadClientListAction(false));
        await Task.CompletedTask;
    }
}
