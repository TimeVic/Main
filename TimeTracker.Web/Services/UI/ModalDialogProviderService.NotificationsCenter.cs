using MudBlazor;
using TimeTracker.Web.Pages.Dashboard.Shared.NotificationCenter;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowNotificationsCenterModal()
    {
        await _mudDialogService.ShowAsync<NotificationCenterModal>("Notifications", new DialogOptions()
        {
            Position = DialogPosition.TopCenter,
            MaxWidth = MaxWidth.Medium
        });
    }
}
