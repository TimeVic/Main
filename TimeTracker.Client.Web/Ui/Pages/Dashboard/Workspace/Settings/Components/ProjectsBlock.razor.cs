using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Web.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class ProjectsBlock
{
    [Inject] 
    private IState<ProjectState> _state { get; set; }

    [Inject]
    private IModalDialogProviderService _modalDialogService { get; set; } = default!;

    private void OnAdd()
    {
        _modalDialogService.ShowAddProjectModal();
    }
    
    private void OnEdit(ProjectDto context)
    {
        _modalDialogService.ShowUpdateProjectModal(context);
    }
}
