using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Clients.Parts;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public Task<AppModalResult> ShowAddClientModal(Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<AddClientModal>(
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

    public Task<AppModalResult> ShowUpdateClientModal(ClientDto client, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<UpdateClientModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(UpdateClientModal.Client)] = client
            },
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

    public Task<AppModalResult> ShowAddProjectModal(Guid? initialClientId = null, Action<AppModalResult>? onClose = null)
    {
        var parameters = new Dictionary<string, object?>();
        if (initialClientId.HasValue) parameters[nameof(AddProjectModal.InitialClientId)] = initialClientId.Value;

        return _appModalDialogService.ShowAsync<AddProjectModal>(
            parameters: parameters,
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

    public Task<AppModalResult> ShowUpdateProjectModal(ProjectDto project, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<UpdateProjectModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(UpdateProjectModal.Project)] = project
            },
            options: new AppModalOptions
            {
                Size = AppModalSize.Medium,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }
}
