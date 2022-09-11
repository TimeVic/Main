using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Pages.Dashboard.Project;

public partial class ProjectPage
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadProjectListAction(1));
    }
}
