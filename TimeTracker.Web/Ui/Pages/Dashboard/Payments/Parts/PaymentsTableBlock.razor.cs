using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Payment;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Payments.Parts;

public partial class PaymentsTableBlock
{
    [Inject]
    public IState<PaymentState> _state { get; set; }

    private bool _isLoading => _state.Value.IsListLoading;
    
    private PaymentDto? _paymentToDelete;
    private PaymentDto? _paymentToUpdate;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private Task OnRowClickHandler(DataGridRowClickEventArgs<PaymentDto> arg)
    {
        _paymentToUpdate = arg.Item;
        return Task.CompletedTask;
    }

    private void OnDeletePayment()
    {
        Dispatcher.Dispatch(new DeletePaymentAction(_paymentToDelete!.Id));
    }
}
