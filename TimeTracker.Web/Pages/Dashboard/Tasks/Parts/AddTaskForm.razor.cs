using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.TasksList;
using LoadListAction = TimeTracker.Web.Store.Tasks.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class AddTaskForm
{
    [Inject]
    public IState<TasksListState> TasksListState { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;

    private bool _isDisabled => TasksListState.Value.SelectedTaskListId == null;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        TasksListState.StateChanged += (data, stateEvent) =>
        {
            model.TaskListId = TasksListState.Value.SelectedTaskListId ?? 0;
        };
    }

    private async Task HandleSubmit(AddRequest request)
    {
        _isLoading = true;
        try
        {
            var responseDto = await ApiService.TasksAddAsync(request);
            if (responseDto != null)
            {
                Dispatcher.Dispatch(new LoadListAction());
                model.Title = "";
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Task has been added"
                });
            }
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Task adding error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
}
