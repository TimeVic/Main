using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<AppModalResult> ShowAddWorkspaceModal(Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowAddWorkspaceMemberModal(Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowUpdateWorkspaceMemberModal(WorkspaceMemberDto member, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowDeleteWorkspaceModal(WorkspaceDto workspace, Action<AppModalResult>? onClose = null);
}
