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

    private GetListFilterRequest model = new();
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(TasksListState.Value.Filter);
    }
    
    private void ResetForm()
    {
        model = new GetListFilterRequest();
        HandleSubmit(model);
    }

    private void LoadList()
    {
        Dispatcher.Dispatch(new LoadListAction(0));
    }

    private void OnChangeAssignedUserId(long userId)
    {
        model.AssignedUserId = userId == 0 ? null : userId;
    }

    private void HandleSubmit(GetListFilterRequest filterData)
    {
        Dispatcher.Dispatch(new SetListFilterAction(filterData));
        LoadList();
    }
}
