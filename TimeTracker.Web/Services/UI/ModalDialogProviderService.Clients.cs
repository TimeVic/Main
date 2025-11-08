using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Web.Pages.Dashboard.Client.Parts.List;
using TimeTracker.Web.Pages.Dashboard.Payment.Parts;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddClientModal()
    {
        var parameters = new DialogParameters
        {
            PrimaryAction = "",
            TrapFocus = false
        };
        await _dialogService.ShowDialogAsync<AddClientModal>(parameters);
    }
}
