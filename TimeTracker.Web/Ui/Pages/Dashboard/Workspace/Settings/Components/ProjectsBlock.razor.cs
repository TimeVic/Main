using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class ProjectsBlock
{
    [Inject] 
    private IState<ProjectState> _state { get; set; }

    private Task OnSave(ProjectDto context)
    {
        if (!string.IsNullOrEmpty(context.Name))
        {
            var updateRequest = new UpdateRequest() { Name = "" };
            updateRequest.Fill(context);
            Dispatcher.Dispatch(new UpdateAction(updateRequest));
        }
        return Task.CompletedTask;
    }
}
