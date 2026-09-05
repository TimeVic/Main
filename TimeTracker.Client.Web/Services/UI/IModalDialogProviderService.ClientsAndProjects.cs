using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<AppModalResult> ShowAddClientModal(Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowUpdateClientModal(ClientDto client, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowAddProjectModal(Guid? initialClientId = null, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowUpdateProjectModal(ProjectDto project, Action<AppModalResult>? onClose = null);
}
