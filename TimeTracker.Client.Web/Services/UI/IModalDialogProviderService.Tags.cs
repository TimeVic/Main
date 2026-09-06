using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<AppModalResult> ShowAddTagModal(Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowUpdateTagModal(TagDto tag, Action<AppModalResult>? onClose = null);
}
