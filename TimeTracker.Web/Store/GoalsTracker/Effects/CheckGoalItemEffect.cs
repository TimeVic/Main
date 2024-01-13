using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.GoalsTracker.Effects;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker.Effects;

public class CheckGoalItemEffect: Effect<CheckGoalItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<CheckGoalItemEffect> _logger;

    public CheckGoalItemEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<CheckGoalItemEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(CheckGoalItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var itemMarks = _state.Value.CurrentTracker.Items.First(item => item.Id == action.Item.Id).CompletionMarkers;
            var existsMark = itemMarks.FirstOrDefault(
                item => item.DayOfMonth == action.DayOfMonth
            );
            if (existsMark == null)
            {
                existsMark = new GoalsTrackerCompletionMarkerDto()
                {
                    DayOfMonth = action.DayOfMonth
                };
                itemMarks.Add(existsMark);
            }
            existsMark.IsChecked = action.IsChecked;
            
            dispatcher.Dispatch(new SetCompletionItemsAction(action.Item, itemMarks));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
