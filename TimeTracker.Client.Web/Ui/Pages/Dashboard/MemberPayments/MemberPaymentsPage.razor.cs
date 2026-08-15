using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.MemberPayments;
using WorkspaceMemberActions = TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.MemberPayments;

public partial class MemberPaymentsPage
{
    [Inject]
    public IState<MemberPaymentState> _state { get; set; }
    
    [Inject]
    public IDispatcher _dispatcher { get; set; }

    [Inject]
    public ISecurityManager SecurityManager { get; set; }

    private bool _isShowAddMemberPaymentModal = false;
    private Guid _memberFilterId = Guid.Empty;
    private bool CanManageOtherMemberPayments =>
        SecurityManager.HasPermission(WorkspacePermission.CreateMemberPaymentForOtherMembers);

    private bool CanCreatePayments => SecurityManager.HasPermission(WorkspacePermission.CreateMemberPayment);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _dispatcher.Dispatch(new LoadMemberPaymentListAction(true));
        _dispatcher.Dispatch(new WorkspaceMemberActions.LoadListAction());
    }

    private void OnMemberFilterChanged(WorkspaceMemberDto? member)
    {
        _memberFilterId = member?.Id ?? Guid.Empty;
        _dispatcher.Dispatch(new SetMemberPaymentSelectedPageAction(1));
        _dispatcher.Dispatch(new LoadMemberPaymentListAction(true, _memberFilterId));
    }

    private void OnPageChanged(int selectedPage)
    {
        _dispatcher.Dispatch(new SetMemberPaymentSelectedPageAction(selectedPage));
        _dispatcher.Dispatch(new LoadMemberPaymentListAction(true, _memberFilterId));
    }
}
