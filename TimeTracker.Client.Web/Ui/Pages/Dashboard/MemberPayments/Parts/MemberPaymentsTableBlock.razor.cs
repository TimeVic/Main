using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.MemberPayments;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.MemberPayments.Parts;

public partial class MemberPaymentsTableBlock
{
    [Inject]
    public IState<MemberPaymentState> _state { get; set; } = null!;

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = null!;

    [Inject]
    private TimeTracker.Client.Web.Services.UI.IModalDialogProviderService _modalDialogService { get; set; } = null!;

    private bool _isLoading => _state.Value.IsListLoading;

    private bool CanUpdatePayments => SecurityManager.HasPermission(WorkspacePermission.UpdateMemberPayment);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private async Task OpenUpdateModal(MemberPaymentDto payment)
    {
        if (!CanUpdatePayments)
        {
            return;
        }

        await _modalDialogService.ShowUpdateMemberPaymentModal(payment);
    }

    private async Task OpenDeleteConfirmation(MemberPaymentDto payment)
    {
        var confirmed = await _modalDialogService.ShowConfirmationAsync(
            DashboardLocalizer["Delete"].Value,
            DashboardLocalizer["AreYouSure"].Value
        );
        if (confirmed)
        {
            Dispatcher.Dispatch(new DeleteMemberPaymentAction(payment.Id));
        }
    }
}
