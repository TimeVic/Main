using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Store.Tasks.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<TasksState> _state;
    private readonly IState<TasksListState> _tasksListState;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
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
        try
        {
            var tasksListId = _tasksListState.Value.SelectedTaskListId;
            if (tasksListId.HasValue)
            {
                dispatcher.Dispatch(new SetIsListLoading(true));
                var response = await _apiService.TasksGetListAsync(new GetListRequest()
                {
                    TaskListId = tasksListId.Value,
                    Filter = _state.Value.Filter
                });
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
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
