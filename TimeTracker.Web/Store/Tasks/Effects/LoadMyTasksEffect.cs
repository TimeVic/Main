using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Dashboard;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Store.Tasks.Effects;

public class LoadMyTasksEffect: Effect<LoadOverdueTasksListAction>
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

    public override async Task HandleAsync(LoadOverdueTasksListAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsOverdueTasksListLoadingAction(true));
            var response = await _apiService.TasksGetMyListAsync(
                _authState.Value.Workspace.Id,
                new List<TaskStatus>()
                {
                    TaskStatus.ToDo,
                    TaskStatus.InProgress
                }
            );
            dispatcher.Dispatch(new SetOverdueTasksListItemsAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsOverdueTasksListLoadingAction(false));
        }
    }
}
