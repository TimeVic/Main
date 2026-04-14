using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.Payment;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Payments;

public partial class PaymentsPage
{
    [Inject]
    public IState<PaymentState> _state { get; set; }

    private bool _isShowAddPaymentModal = false;
}
