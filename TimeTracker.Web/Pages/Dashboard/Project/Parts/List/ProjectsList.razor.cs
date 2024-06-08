using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TimeEntry;
using LoadListAction = TimeTracker.Web.Store.Project.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Project.Parts.List;

public partial class ProjectsList
{
    [Inject] 
    private IState<ProjectState> _state { get; set; }
    
    [Inject] 
    private ModalDialogProviderService _dialogService { get; set; }
    
    [Inject]
    public ILogger<ProjectsList> _logger { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadListAction(true));
    }

    private Task NavigateToProduct(ProjectDto item)
    {
        NavigationManager.NavigateTo(string.Format(SiteUrl.Dashboard_Project, item.Id));
        return Task.CompletedTask;
    }

    private async Task DeleteProject(ProjectDto item)
    {
        var isOk = await _dialogService.ShowDeleteConfirmationDialog(
            "Are you sure you want to remove this project?"    
        );
        if (!isOk.HasValue || !isOk.Value)
        {
            return;
        }
        try
        {
            await ApiService.ProjectDeleteAsync(item.Id);
            Dispatcher.Dispatch(new LoadListAction(true));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            await ToastService.ShowError("Project deletion error");
        }
    }

    private async Task OnAddProject()
    {
        await _dialogService.ShowAddProjectModal();
    }

    private async Task OnEditProject(ProjectDto item)
    {
        await _dialogService.ShowUpdateProjectModal(item);
    }
}
