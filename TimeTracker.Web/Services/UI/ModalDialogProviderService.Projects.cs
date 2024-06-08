using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Pages.Dashboard.Payment.Parts;
using TimeTracker.Web.Pages.Dashboard.Project.Parts.List;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddProjectModal()
    {
        await _mudDialogService.ShowAsync<AddProjectModal>("Add new project");
    }
    
    public async Task ShowUpdateProjectModal(ProjectDto item)
    {
        var parameters = new DialogParameters<UpdateProjectModal>
        {
            { x => x.Project, item },
        };
        await _mudDialogService.ShowAsync<UpdateProjectModal>("Update payment", parameters);
    }
}
