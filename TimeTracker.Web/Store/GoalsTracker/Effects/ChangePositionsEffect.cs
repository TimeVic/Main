using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.GoalsTracker.Effects;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker.Effects;

public class ChangePositionsEffect: Effect<ChangePositionsAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<ChangePositionsEffect> _logger;

    public ChangePositionsEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<ChangePositionsEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(ChangePositionsAction action, IDispatcher dispatcher)
    {
        try
        {
            var position = 0;
            foreach (var goal in action.Goals)
            {
                goal.Position = position++;
            }
            await _apiService.GoalsTrackerChangePositionsAsync(
                _authState.Value.Workspace.Id,
                new DateTime(_state.Value.CurrentTracker.Year, _state.Value.CurrentTracker.Month, 1),
                action.Goals
            );
            dispatcher.Dispatch(new SetGoalsTrackerItemsAction(action.Goals));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
