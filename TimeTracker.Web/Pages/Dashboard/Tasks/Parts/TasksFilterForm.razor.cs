using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksFilterForm
{
    [Inject]
    public IState<TasksState> TasksListState { get; set; }

    public GetListFilterRequest Filter
    {
        get => TasksListState.Value.Filter;
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
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

    private void OnChangeSearchString(string searchString)
    {
        Filter.SearchString = searchString;
        LoadList();
    }

    private void OnChangeIsArchived(bool? isArchived)
    {
        Filter.IsArchived = isArchived;
        LoadList();
    }
    
    private void OnChangeAssignedUserId(long userId)
    {
        Filter.AssignedUserId = userId == 0 ? null : userId;
        LoadList();
    }
}
