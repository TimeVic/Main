using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Dashboard;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Client.Core.Store.Tasks.Effects;

public class UpdateTaskEffect: Effect<UpdateTaskAction>
{
    private readonly IState<TasksState> _state;
    private readonly IState<TasksListState> _tasksListState;
    private readonly IApiService _apiService;
    private readonly ILogger<UpdateTaskEffect> _logger;
    private readonly IToastService _toastService;

    public UpdateTaskEffect(
        IApiService apiService,
        IState<TasksState> state,
        IState<TasksListState> tasksListState,
        ILogger<UpdateTaskEffect> logger,
        IToastService toastService
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
                // Keep archive and restore updates aligned with the visible task-list filter.
                if (!IsIncludedInSelectedTaskList(response, selectedTaskListId))
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

    private bool IsIncludedInSelectedTaskList(TaskDto task, Guid? selectedTaskListId)
    {
        if (!selectedTaskListId.HasValue || task.TaskList.Id != selectedTaskListId.Value)
        {
            return false;
        }

        var filter = _state.Value.Filter;
        if (filter.IsArchived.HasValue && task.IsArchived != filter.IsArchived.Value)
        {
            return false;
        }

        if (filter.AssignedUserId.HasValue && task.User?.Id != filter.AssignedUserId.Value)
        {
            return false;
        }

        if (filter.Status.HasValue && task.Status != filter.Status.Value)
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(filter.SearchString)
            || Contains(task.Title, filter.SearchString)
            || Contains(task.Description, filter.SearchString);
    }

    private static bool Contains(string? value, string searchString)
    {
        return value?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
