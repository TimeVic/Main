using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TasksList.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<TasksListState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<TasksListState> state,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        try
        {
            var isLoad = action.IsReload
                         || !_state.Value.IsLoaded
                         || _state.Value.LoadedProjectId != action.ProjectId;
            if (!isLoad)
            {
                return;
            }

            dispatcher.Dispatch(new SetIsListLoadingAction(true));
            var response = await _apiService.TaskListGetListAsync(new GetListRequest()
            {
                WorkspaceId = _authState.Value.Workspace!.Id,
                ProjectId = action.ProjectId
            });
            dispatcher.Dispatch(new SetListItemsAction(response, action.ProjectId));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoadingAction(false));
        }
    }
}
