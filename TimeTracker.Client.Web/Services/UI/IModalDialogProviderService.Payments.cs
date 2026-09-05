using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<AppModalResult> ShowViewClientPaymentModal(ClientPaymentDto payment, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowAddClientPaymentModal(Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowUpdateClientPaymentModal(ClientPaymentDto payment, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowAddMemberPaymentModal(
        Guid? initialMemberId = null,
        decimal? initialAmount = null,
        Guid? initialProjectId = null,
        Action<AppModalResult>? onClose = null
    );

    Task<AppModalResult> ShowUpdateMemberPaymentModal(MemberPaymentDto payment, Action<AppModalResult>? onClose = null);
}
