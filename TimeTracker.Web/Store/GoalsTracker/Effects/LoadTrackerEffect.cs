using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.GoalsTracker.Effects;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker.Effects;

public class LoadTrackerEffect: Effect<LoadTrackerAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadTrackerEffect> _logger;

    public LoadTrackerEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<LoadTrackerEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadTrackerAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetIsListLoadingAction(true));
        try
        {
            var tracker = await _apiService.GoalsTrackerLoadAsync(_authState.Value.Workspace.Id, action.Date);
            dispatcher.Dispatch(new SetTrackerAction(tracker));
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
