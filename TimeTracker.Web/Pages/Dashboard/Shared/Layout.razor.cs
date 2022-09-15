using TimeTracker.Web.Store.Client;
using TimeTracker.Web.Store.Project;

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
        Dispatcher.Dispatch(new LoadProjectListAction(false));
        Dispatcher.Dispatch(new LoadClientListAction(false));
        return Task.CompletedTask;
    }
}
