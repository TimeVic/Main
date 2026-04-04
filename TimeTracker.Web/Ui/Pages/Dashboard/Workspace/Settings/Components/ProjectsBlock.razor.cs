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

    private ProjectDto? _projectToUpdate { get; set; }
    
    private Task OnEdit(ProjectDto context)
    {
        _projectToUpdate = context;
        return Task.CompletedTask;
    }
}
