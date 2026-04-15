using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.Payment;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Payments;

public partial class PaymentsPage
{
    [Inject]
    public IState<PaymentState> _state { get; set; }
    
    [Inject]
    public IDispatcher _dispatcher { get; set; }

    private bool _isShowAddPaymentModal = false;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _dispatcher.Dispatch(new LoadPaymentListAction(true));
    }
}
