using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.ClientPayments.Parts.Modals;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.MemberPayments.Parts.Modals;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public Task<AppModalResult> ShowViewClientPaymentModal(ClientPaymentDto payment, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<ViewClientPaymentModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(ViewClientPaymentModal.ClientPayment)] = payment
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

    public Task<AppModalResult> ShowAddClientPaymentModal(Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<AddClientPaymentModal>(
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

    public Task<AppModalResult> ShowUpdateClientPaymentModal(ClientPaymentDto payment, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<UpdateClientPaymentModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(UpdateClientPaymentModal.ClientPayment)] = payment
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

    public Task<AppModalResult> ShowAddMemberPaymentModal(
        Guid? initialMemberId = null,
        decimal? initialAmount = null,
        Guid? initialProjectId = null,
        Action<AppModalResult>? onClose = null
    )
    {
        var parameters = new Dictionary<string, object?>();
        if (initialMemberId.HasValue && initialMemberId.Value != Guid.Empty)
        {
            parameters[nameof(AddMemberPaymentModal.InitialMemberId)] = initialMemberId.Value;
        }
        if (initialAmount.HasValue && initialAmount.Value > 0)
        {
            parameters[nameof(AddMemberPaymentModal.InitialAmount)] = initialAmount.Value;
        }
        if (initialProjectId.HasValue && initialProjectId.Value != Guid.Empty)
        {
            parameters[nameof(AddMemberPaymentModal.InitialProjectId)] = initialProjectId.Value;
        }

        return _appModalDialogService.ShowAsync<AddMemberPaymentModal>(
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

    public Task<AppModalResult> ShowUpdateMemberPaymentModal(MemberPaymentDto payment, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<UpdateMemberPaymentModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(UpdateMemberPaymentModal.MemberPayment)] = payment
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
