using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Client.Core.Store.Tasks.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<TasksState> _state;
    private readonly IState<TasksListState> _tasksListState;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        IApiService apiService,
        IState<TasksState> state,
        IState<TasksListState> tasksListState,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _state = state;
        _tasksListState = tasksListState;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        var taskListId = action.TaskListId;
        var filter = action.Filter ?? _state.Value.Filter;
        try
        {
            // Ignore responses for a task list or filter that has been replaced while loading.
            if (!IsCurrentRequest(taskListId, filter))
            {
                return;
            }

            if (taskListId.HasValue)
            {
                dispatcher.Dispatch(new SetIsListLoading(true));
                var response = await _apiService.TasksGetListAsync(new GetListRequest()
                {
                    TaskListId = taskListId.Value,
                    Filter = filter
                });

                if (!IsCurrentRequest(taskListId, filter))
                {
                    return;
                }

                if (response == null)
                {
                    dispatcher.Dispatch(new SetListItemsAction(new GetListResponse(new List<TaskDto>(), 0)));
                    return;
                }

                // Keep the task-list counters loaded by the task-list endpoint.
                dispatcher.Dispatch(new SetListItemsAction(response));
            }
            else
            {
                dispatcher.Dispatch(
                    new SetListItemsAction(
                        new GetListResponse(new List<TaskDto>(), 0)
                    )
                );
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            if (IsCurrentRequest(taskListId, filter))
            {
                dispatcher.Dispatch(new SetIsListLoading(false));
            }
        }
    }

    private bool IsCurrentRequest(Guid? taskListId, GetListFilterRequest filter)
    {
        return _tasksListState.Value.SelectedTaskListId == taskListId
            && HasSameFilterValues(_state.Value.Filter, filter);
    }

    private static bool HasSameFilterValues(GetListFilterRequest currentFilter, GetListFilterRequest requestFilter)
    {
        return currentFilter.AssignedUserId == requestFilter.AssignedUserId
            && string.Equals(currentFilter.SearchString, requestFilter.SearchString, StringComparison.Ordinal)
            && currentFilter.IsArchived == requestFilter.IsArchived
            && currentFilter.Status == requestFilter.Status;
    }
}
