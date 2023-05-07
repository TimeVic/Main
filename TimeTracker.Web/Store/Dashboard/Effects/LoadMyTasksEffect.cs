using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Store.Dashboard.Effects;

public class LoadMyTasksEffect: Effect<LoadTasksListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<DashboardState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadMyTasksEffect> _logger;

    public LoadMyTasksEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<DashboardState> state,
        ILogger<LoadMyTasksEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadTasksListAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsTasksListLoadingAction(true));
            var response = await _apiService.TasksGetMyListAsync(
                _authState.Value.Workspace.Id,
                new List<TaskStatus>()
                {
                    TaskStatus.ToDo,
                    TaskStatus.InProgress
                }
            );
            dispatcher.Dispatch(new SetTasksListItemsAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsTasksListLoadingAction(false));
        }
    }
}
