using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Dashboard;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Store.Tasks.Effects;

public class UpdateTaskEffect: Effect<UpdateTaskAction>
{
    private readonly IState<TasksState> _state;
    private readonly IState<TasksListState> _tasksListState;
    private readonly ApiService _apiService;
    private readonly ILogger<UpdateTaskEffect> _logger;
    private readonly ToastService _toastService;

    public UpdateTaskEffect(
        ApiService apiService,
        IState<TasksState> state,
        IState<TasksListState> tasksListState,
        ILogger<UpdateTaskEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _state = state;
        _tasksListState = tasksListState;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(UpdateTaskAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsTaskSavingAction(true));

            var response = await _apiService.TasksUpdateAsync(action.UpdateRequest);
            if (response == null)
            {
                return;
            }

            if (action.IsUpdateState)
            {
                dispatcher.Dispatch(new SetOverdueTasksListItemAction(response));
                var selectedTaskListId = _tasksListState.Value.SelectedTaskListId;
                // Keep the current task list view accurate when a task is moved to another task list.
                if (selectedTaskListId.HasValue && response.TaskList.Id != selectedTaskListId.Value)
                {
                    dispatcher.Dispatch(new RemoveListItemAction(response.Id));
                }
                else
                {
                    dispatcher.Dispatch(new SetListItemAction(response));
                }

                if (action.IsShowToast)
                {
                    _toastService.ShowInfo($"Task {response.FormattedId} updated.");
                }
            }
        }
        catch (Exception e)
        {
            _toastService.ShowError("Task adding error");
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsTaskSavingAction(false));
        }
    }
}
