using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Pages.Dashboard.Workspace;

public partial class WorkspacesPage
{
    [Inject]
    public IState<WorkspaceState> _state { get; set; }
    
    private RadzenDataGrid<WorkspaceDto> _grid;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    
    private async Task InsertRow()
    {
        Dispatcher.Dispatch(new AddEmptyListItemAction());
        // await _grid.GoToPage(0);
        await EditRow(_state.Value.ItemToAdd);
    }
    
    private async Task EditRow(WorkspaceDto item)
    {
        await _grid.EditRow(item);
    }

    private async Task OnClickSaveRow(WorkspaceDto item)
    {
        await _grid.UpdateRow(item);
    }

    private void OnClickCancelEditMode(WorkspaceDto item)
    {
        Dispatcher.Dispatch(new RemoveEmptyListItemAction());
        _grid.CancelEditRow(item);
    }
    
    private async Task OnUpdateRow(WorkspaceDto item)
    {
        if (item.Id > 0)
        {
            Dispatcher.Dispatch(new UpdateWorkspaceAction(item));
            return;
        }

        Dispatcher.Dispatch(new SaveEmptyListItemAction());
    }
}
