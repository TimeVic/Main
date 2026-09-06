using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.ClientPayments;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.ClientPayments.Parts;

public partial class ClientPaymentsTableBlock
{
    [Parameter]
    public required IReadOnlyCollection<ClientPaymentDto> Items { get; set; }

    [Inject]
    public IState<ClientPaymentState> _state { get; set; }

    [Inject]
    public ISecurityManager SecurityManager { get; set; }

    [Inject]
    private TimeTracker.Client.Web.Services.UI.IModalDialogProviderService _modalDialogService { get; set; } = null!;

    private bool _isLoading => _state.Value.IsListLoading;

    private bool CanUpdatePayments => SecurityManager.HasPermission(WorkspacePermission.UpdateClientPayment);

    private async Task OpenViewModal(ClientPaymentDto payment)
    {
        await _modalDialogService.ShowViewClientPaymentModal(payment);
    }

    private async Task OpenUpdateModal(ClientPaymentDto payment)
    {
        await _modalDialogService.ShowUpdateClientPaymentModal(payment);
    }

    private async Task OpenDeleteConfirmation(ClientPaymentDto payment)
    {
        var confirmed = await _modalDialogService.ShowConfirmationAsync(
            DashboardLocalizer["Delete"].Value,
            DashboardLocalizer["AreYouSure"].Value
        );
        if (confirmed)
        {
            Dispatcher.Dispatch(new DeleteClientPaymentAction(payment.Id));
        }
    }
}
