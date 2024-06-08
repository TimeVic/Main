using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Pages.Dashboard.Client.Parts.List;
using TimeTracker.Web.Pages.Dashboard.Payment.Parts;
using TimeTracker.Web.Pages.Dashboard.Project.Parts.List;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddClientModal()
    {
        await _mudDialogService.ShowAsync<AddClientModal>("Add new client");
    }
}
