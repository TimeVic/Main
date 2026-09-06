using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Integrations;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<AppModalResult> ShowRejectReasonModal(EventCallback<string>? onConfirm = null, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowClientShareReportModal(Guid clientId, string clientName, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowSupportModal(Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowAddChannelModal(Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowChangePasswordModal(Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowIntegrationHelpModal(IntegrationHelpInfo helpInfo, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowEmojiPickerModal(EventCallback<string>? onEmojiSelected = null, Action<AppModalResult>? onClose = null);
}
