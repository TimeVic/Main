using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Web.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class SecurityBlock
{
    [Inject]
    private IModalDialogProviderService _modalDialogService { get; set; } = default!;

    private Task OpenChangePasswordModal()
    {
        return _modalDialogService.ShowChangePasswordModal();
    }
}
