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
            if (action.IsUpdateState)
            {
                dispatcher.Dispatch(new SetOverdueTasksListItemAction(response));
                dispatcher.Dispatch(new SetListItemAction(response));
                _toastService.ShowInfo($"Task {response.FormattedId} updated.");
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
