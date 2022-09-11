using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Project.Parts.List;

public partial class ProjectsList
{
    [Inject] 
    private IState<ProjectState> _state { get; set; }
    
    private RadzenDataGrid<ProjectDto> _grid;
    
    private Task OnLoadList(LoadDataArgs arg)
    {
        Dispatcher.Dispatch(new LoadProjectListAction(arg.Skip ?? 0));
        return Task.CompletedTask;
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
    
    private async Task EditRow(ProjectDto app)
    {
        await _grid.EditRow(app);
    }

    private async Task OnClickSaveRow(ProjectDto app)
    {
        await _grid.UpdateRow(app);
    }

    private void OnClickCancelEditMode(ProjectDto app)
    {
        Dispatcher.Dispatch(new RemoveEmptyProjectListItemAction());
        _grid.CancelEditRow(app);
    }
}
