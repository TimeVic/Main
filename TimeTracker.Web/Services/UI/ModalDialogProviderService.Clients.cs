using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Web.Ui.Pages.Dashboard.Client.Parts.List;

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
