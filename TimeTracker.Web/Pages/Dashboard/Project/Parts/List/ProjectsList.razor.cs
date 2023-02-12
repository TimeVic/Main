using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
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
        Dispatcher.Dispatch(new LoadListAction(true));
    }

    private async Task OnDeleteItemAsync(ProjectDto value)
    {
        await Task.CompletedTask;
    }
    
    private async Task InsertRow()
    {
        Dispatcher.Dispatch(new AddEmptyProjectListItemAction());
        // await _grid.GoToPage(0);
        await EditNewRow(_state.Value.ItemToAdd);
    }
    
    private async Task EditNewRow(ProjectDto item)
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
        Dispatcher.Dispatch(new SaveEmptyProjectListItemAction());
    }
    
    private Task NavigateToProduct(ProjectDto item)
    {
        NavigationManager.NavigateTo(string.Format(SiteUrl.Dashboard_Project, item.Id));
        return Task.CompletedTask;
    }
}
