using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Project.Parts.List;

public partial class ProjectsList
{
    [Inject] 
    private IState<ProjectState> _state { get; set; }
    
    private RadzenDataGrid<ProjectDto> _grid;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadProjectListAction(true));
    }

    private async Task OnDeleteItemAsync(ProjectDto value)
    {
        await Task.CompletedTask;
    }
    
    private async Task InsertRow()
    {
        Dispatcher.Dispatch(new AddEmptyProjectListItemAction());
        // await _grid.GoToPage(0);
        await EditRow(_state.Value.ItemToAdd);
    }
    
    private async Task EditRow(ProjectDto item)
    {
        await _grid.EditRow(item);
    }

    private async Task OnClickSaveRow(ProjectDto item)
    {
        await _grid.UpdateRow(item);
    }

    private void OnClickCancelEditMode(ProjectDto item)
    {
        Dispatcher.Dispatch(new RemoveEmptyProjectListItemAction());
        _grid.CancelEditRow(item);
    }
    
    private async Task OnUpdateRow(ProjectDto item)
    {
        if (item.Id > 0)
        {
            // await UpdateApplication(item);
            return;
        }

        Dispatcher.Dispatch(new SaveEmptyProjectListItemAction());
    }
}
