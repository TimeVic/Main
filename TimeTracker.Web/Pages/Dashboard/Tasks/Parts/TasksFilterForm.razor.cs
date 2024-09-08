using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;
using LoadListAction = TimeTracker.Web.Store.Tasks.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksFilterForm
{
    [Inject]
    public IState<TasksState> TasksState { get; set; }

    [Inject]
    public IState<TasksListState> TasksListState { get; set; }
    
    public GetListFilterRequest Filter
    {
        get => TasksState.Value.Filter;
    }

    public TaskListDto? _selectedTaskList
    {
        get => TasksListState.Value.SelectedTaskList;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (_selectedTaskList != null)
        {
            LoadList();
        }
    }
    
    private void ResetForm()
    {
        Dispatcher.Dispatch(new SetListFilterAction(new GetListFilterRequest()));
    }

    private void LoadList()
    {
        Dispatcher.Dispatch(new SetListFilterAction(Filter));
        Dispatcher.Dispatch(new LoadListAction());
    }

    private void HandleSubmit(GetListFilterRequest filterData)
    {
        Dispatcher.Dispatch(new SetListFilterAction(filterData));
    }

    private void OnChangeSearchString(string? searchString)
    {
        Filter.SearchString = searchString;
        LoadList();
    }

    private void OnChangeIsArchived(bool? isArchived)
    {
        Filter.IsArchived = isArchived;
        LoadList();
    }
    
    private void OnChangeAssignedUser(WorkspaceMembershipDto? membership)
    {
        Filter.AssignedUserId = membership?.Id;
        LoadList();
    }
}
