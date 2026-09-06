using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class DangerZoneBlock
{
    [Inject]
    private IModalDialogProviderService _modalDialogService { get; set; } = default!;

    private async Task RequestDeletionAsync()
    {
        var confirmed = await _modalDialogService.ShowConfirmationAsync(new AppConfirmationOptions
        {
            Title = DashboardLocalizer["UserSettings_RequestDeletionConfirmTitle"].Value,
            Message = DashboardLocalizer["UserSettings_RequestDeletionConfirmMessage"].Value,
            ConfirmText = DashboardLocalizer["UserSettings_RequestDeletion"].Value,
            Type = AppConfirmationType.Alert
        });

        if (confirmed)
        {
            OnRequestDeletionConfirmed();
        }
    }

    private void OnRequestDeletionConfirmed()
    {
        // TODO: Wire to account deletion request API
    }
}
