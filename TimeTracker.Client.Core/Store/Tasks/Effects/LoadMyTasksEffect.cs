using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Dashboard;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Core.Store.Tasks.Effects;

public class LoadOverdueTasksEffect: Effect<LoadOverdueTasksListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<DashboardState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadOverdueTasksEffect> _logger;

    public LoadOverdueTasksEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<DashboardState> state,
        ILogger<LoadOverdueTasksEffect> logger
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
            var response = await _apiService.TasksGetOverdueListAsync(
                _authState.Value.Workspace!.Id
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
