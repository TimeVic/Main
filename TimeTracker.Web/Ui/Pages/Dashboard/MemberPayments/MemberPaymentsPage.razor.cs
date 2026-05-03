using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.MemberPayments;

namespace TimeTracker.Web.Ui.Pages.Dashboard.MemberPayments;

public partial class MemberPaymentsPage
{
    [Inject]
    public IState<MemberPaymentState> _state { get; set; }
    
    [Inject]
    public IDispatcher _dispatcher { get; set; }

    private bool _isShowAddMemberPaymentModal = false;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _dispatcher.Dispatch(new LoadMemberPaymentListAction(true));
    }
}
