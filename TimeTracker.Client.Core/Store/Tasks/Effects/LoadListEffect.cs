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
        try
        {
            // Each action targets the list selected by the route when it was dispatched.
            if (_tasksListState.Value.SelectedTaskListId != taskListId)
            {
                return;
            }

            if (taskListId.HasValue)
            {
                dispatcher.Dispatch(new SetIsListLoading(true));
                var response = await _apiService.TasksGetListAsync(new GetListRequest()
                {
                    TaskListId = taskListId.Value,
                    Filter = _state.Value.Filter
                });

                // Ignore a response for a list that was deselected while its request was in flight.
                if (_tasksListState.Value.SelectedTaskListId != taskListId)
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
            if (_tasksListState.Value.SelectedTaskListId == taskListId)
            {
                dispatcher.Dispatch(new SetIsListLoading(false));
            }
        }
    }
}
