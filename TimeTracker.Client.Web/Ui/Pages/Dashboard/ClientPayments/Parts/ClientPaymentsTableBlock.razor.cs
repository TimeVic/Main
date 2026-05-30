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

    private bool _isLoading => _state.Value.IsListLoading;

    private bool CanUpdatePayments => SecurityManager.HasPermission(WorkspacePermission.UpdateClientPayment);

    private ClientPaymentDto? _paymentToDelete;
    private ClientPaymentDto? _paymentToUpdate;
    private ClientPaymentDto? _paymentToView;

    private void OnDeleteClientPayment()
    {
        Dispatcher.Dispatch(new DeleteClientPaymentAction(_paymentToDelete!.Id));
        _paymentToDelete = null;
    }
}
