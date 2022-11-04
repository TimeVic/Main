
namespace TimeTracker.Web.Pages.Dashboard.Shared;

public partial class Layout
{
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = true;
        await base.OnInitializedAsync();
    }
    
    protected override Task OnAppInitializedAsync()
    {
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Project.LoadProjectListAction(false));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Client.LoadClientListAction(false));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Workspace.LoadListAction(false));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.WorkspaceMemberships.LoadListAction(false));
        return Task.CompletedTask;
    }
}
