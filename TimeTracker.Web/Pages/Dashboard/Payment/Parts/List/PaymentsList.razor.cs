using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Client;
using TimeTracker.Web.Store.Payment;

namespace TimeTracker.Web.Pages.Dashboard.Payment.Parts.List;

public partial class PaymentsList
{
    [Inject] 
    private IState<PaymentState> _state { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadPaymentListAction(true));
    }

    private async Task OnDeleteItem(PaymentDto item)
    {
        var isOk = await ModalDialogService.ShowDeleteConfirmationDialog(
            "Are you sure you want to remove this payment?"
        );
        if (isOk.HasValue && isOk.Value)
        {
            Dispatcher.Dispatch(new DeletePaymentAction(item.Id));
        }
    }

    private async Task OnAddPayment()
    {
        await ModalDialogService.ShowAddPaymentModal();
    }

    private async Task OnEditPayment(PaymentDto item)
    {
        await ModalDialogService.ShowUpdatePaymentModal(item);
    }
}
