using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.MemberPayments;

namespace TimeTracker.Web.Ui.Pages.Dashboard.MemberPayments.Parts;

public partial class MemberPaymentsTableBlock
{
    [Inject]
    public IState<MemberPaymentState> _state { get; set; }

    private bool _isLoading => _state.Value.IsListLoading;
    
    private MemberPaymentDto? _paymentToDelete;
    private MemberPaymentDto? _paymentToUpdate;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private Task OnRowClickHandler(DataGridRowClickEventArgs<MemberPaymentDto> arg)
    {
        _paymentToUpdate = arg.Item;
        return Task.CompletedTask;
    }

    private void OnDeleteMemberPayment()
    {
        Dispatcher.Dispatch(new DeleteMemberPaymentAction(_paymentToDelete!.Id));
    }
}
