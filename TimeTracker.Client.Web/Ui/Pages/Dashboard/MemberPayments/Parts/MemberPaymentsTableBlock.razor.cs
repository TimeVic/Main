using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.MemberPayments;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.MemberPayments.Parts;

public partial class MemberPaymentsTableBlock
{
    [Inject]
    public IState<MemberPaymentState> _state { get; set; }

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = null!;

    private bool _isLoading => _state.Value.IsListLoading;
    
    private MemberPaymentDto? _paymentToDelete;
    private MemberPaymentDto? _paymentToUpdate;

    private bool CanUpdatePayments => SecurityManager.HasPermission(WorkspacePermission.UpdateMemberPayment);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private Task OnRowClickHandler(DataGridRowClickEventArgs<MemberPaymentDto> arg)
    {
        if (!CanUpdatePayments)
        {
            return Task.CompletedTask;
        }

        _paymentToUpdate = arg.Item;
        return Task.CompletedTask;
    }

    private void OnDeleteMemberPayment()
    {
        Dispatcher.Dispatch(new DeleteMemberPaymentAction(_paymentToDelete!.Id));
    }
}
