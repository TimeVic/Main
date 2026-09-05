using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Chat.Parts.Channels;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Approvals.Components;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.Shared;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Modals;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.User.Settings.Components;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Integrations;
using TimeTracker.Client.Web.Ui.Shared.Components.Dialogs;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public Task<AppModalResult> ShowRejectReasonModal(EventCallback<string>? onConfirm = null, Action<AppModalResult>? onClose = null)
    {
        var parameters = new Dictionary<string, object?>();
        if (onConfirm.HasValue)
        {
            parameters[nameof(RejectReasonModal.OnConfirm)] = onConfirm.Value;
        }

        return _appModalDialogService.ShowAsync<RejectReasonModal>(
            parameters: parameters,
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

    public Task<AppModalResult> ShowClientShareReportModal(Guid clientId, string clientName, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<ClientShareReportModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(ClientShareReportModal.ClientId)] = clientId,
                [nameof(ClientShareReportModal.ClientName)] = clientName
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

    public Task<AppModalResult> ShowSupportModal(Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<SupportModal>(
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

    public Task<AppModalResult> ShowAddChannelModal(Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<AddChannelModal>(
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

    public Task<AppModalResult> ShowChangePasswordModal(Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<ChangePasswordModal>(
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

    public Task<AppModalResult> ShowIntegrationHelpModal(IntegrationHelpInfo helpInfo, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<IntegrationHelpModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(IntegrationHelpModal.HelpInfo)] = helpInfo
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

    public Task<AppModalResult> ShowEmojiPickerModal(EventCallback<string>? onEmojiSelected = null, Action<AppModalResult>? onClose = null)
    {
        var parameters = new Dictionary<string, object?>();
        if (onEmojiSelected.HasValue)
        {
            parameters[nameof(EmojiPickerModal.OnEmojiSelected)] = onEmojiSelected.Value;
        }

        return _appModalDialogService.ShowAsync<EmojiPickerModal>(
            parameters: parameters,
            options: new AppModalOptions
            {
                Size = AppModalSize.Full,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }
}
