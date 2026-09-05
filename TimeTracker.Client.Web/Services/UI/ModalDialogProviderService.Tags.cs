using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tags;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Tags.Parts;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public Task<AppModalResult> ShowAddTagModal(Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<AddTagModal>(
            options: new AppModalOptions
            {
                Size = AppModalSize.Small,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowUpdateTagModal(TagDto tag, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<UpdateTagModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(UpdateTagModal.Tag)] = tag
            },
            options: new AppModalOptions
            {
                Size = AppModalSize.Large,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }
}
