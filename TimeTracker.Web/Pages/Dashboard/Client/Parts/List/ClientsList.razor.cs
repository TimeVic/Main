using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Client;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Pages.Dashboard.Client.Parts.List;

public partial class ClientsList
{
    [Inject] 
    private IState<ClientState> _state { get; set; }
    
    private RadzenDataGrid<ClientDto> _grid;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadClientListAction(true));
    }

    private async Task OnDeleteItemAsync(ClientDto value)
    {
        await Task.CompletedTask;
    }
    
    private async Task InsertRow()
    {
        Dispatcher.Dispatch(new AddEmptyClientListItemAction());
        // await _grid.GoToPage(0);
        await EditRow(_state.Value.ItemToAdd);
    }
    
    private async Task EditRow(ClientDto item)
    {
        await _grid.EditRow(item);
    }

    private async Task OnClickSaveRow(ClientDto item)
    {
        await _grid.UpdateRow(item);
    }

    private void OnClickCancelEditMode(ClientDto item)
    {
        Dispatcher.Dispatch(new RemoveEmptyClientListItemAction());
        _grid.CancelEditRow(item);
    }
    
    private async Task OnUpdateRow(ClientDto item)
    {
        if (item.Id > 0)
        {
            // await UpdateApplication(item);
            return;
        }

        Dispatcher.Dispatch(new SaveEmptyClientListItemAction());
    }
}
