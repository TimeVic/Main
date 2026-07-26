using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class ProjectsBlock
{
    [Inject] 
    private IState<ProjectState> _state { get; set; }

    private bool _isAddProjectModalOpened { get; set; }
    private ProjectDto? _projectToUpdate { get; set; }
    private ProjectDto? _projectToDelete { get; set; }

    private Task OnAdd()
    {
        _isAddProjectModalOpened = true;
        return Task.CompletedTask;
    }
    
    private Task OnEdit(ProjectDto context)
    {
        _projectToUpdate = context;
        return Task.CompletedTask;
    }

    private Task OnDeleteClicked(ProjectDto context)
    {
        _projectToDelete = context;
        return Task.CompletedTask;
    }

    private Task OnConfirmDelete()
    {
        if (_projectToDelete != null)
        {
            Dispatcher.Dispatch(new DeleteItemAction(_projectToDelete));
            _projectToDelete = null;
        }

        return Task.CompletedTask;
    }

    private Task OnCloseDeleteConfirmation()
    {
        _projectToDelete = null;
        return Task.CompletedTask;
    }
}
