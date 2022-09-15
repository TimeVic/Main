using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Client;
using TimeTracker.Web.Store.Payment;

namespace TimeTracker.Web.Pages.Dashboard.Payment.Parts.List;

public partial class PaymentsList
{
    [Inject] 
    private IState<PaymentState> _state { get; set; }
    
    private RadzenDataGrid<PaymentDto> _grid;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadPaymentListAction(true));
    }

    private async Task OnDeleteItemAsync(PaymentDto value)
    {
        await Task.CompletedTask;
    }
    
    private async Task InsertRow()
    {
        Dispatcher.Dispatch(new AddEmptyPaymentListItemAction());
        // await _grid.GoToPage(0);
        await EditRow(_state.Value.ItemToAdd);
    }
    
    private async Task EditRow(PaymentDto item)
    {
        await _grid.EditRow(item);
    }

    private async Task OnClickSaveRow(PaymentDto item)
    {
        if (item.Client.Id == 0)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Summary = "Client is required",
                Severity = NotificationSeverity.Error
            });
            return;
        }

        await _grid.UpdateRow(item);
    }

    private void OnClickCancelEditMode(PaymentDto item)
    {
        Dispatcher.Dispatch(new RemoveEmptyPaymentListItemAction());
        _grid.CancelEditRow(item);
    }
    
    private async Task OnUpdateRow(PaymentDto item)
    {
        if (item.Id > 0)
        {
            // await UpdateApplication(item);
            return;
        }

        Dispatcher.Dispatch(new SaveEmptyPaymentListItemAction());
    }
    
    private void OnClientSelected(ClientDto client)
    {
        _state.Value.ItemToAdd.Client = client;
        _state.Value.ItemToAdd.Project = null;
        Dispatcher.Dispatch(new SetPaymentListItemAction(_state.Value.ItemToAdd));
    }

    private void OnProjectSelected(ProjectDto project)
    {
        _state.Value.ItemToAdd.Project = project;
        Dispatcher.Dispatch(new SetPaymentListItemAction(_state.Value.ItemToAdd));
    }
}
