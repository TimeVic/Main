using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Ui.Pages.Dashboard.Project.Parts.List;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddProjectModal()
    {
        var parameters = new DialogParameters
        {
            PrimaryAction = "",
            TrapFocus = false
        };
        await _dialogService.ShowDialogAsync<AddProjectModal>(parameters);
    }
    
    public async Task ShowUpdateProjectModal(ProjectDto item)
    {
        var parameters = new DialogParameters
        {
            PrimaryAction = "",
            TrapFocus = false
        };
        await _dialogService.ShowDialogAsync<UpdateProjectModal>(
            new UpdateProjectModal.Parameters()
            {
                Project = item,
            },
            parameters
        );
    }
}
