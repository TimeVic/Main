using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.GoalsTracker.Effects;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker.Effects;

public class SetCompletionEffect: Effect<SetItemCompletionAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<SetCompletionEffect> _logger;

    public SetCompletionEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<SetCompletionEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(SetItemCompletionAction completionAction, IDispatcher dispatcher)
    {
        try
        {
            var completionMarker = await _apiService.GoalsTrackerSetCompletionAsync(
                completionAction.Item.Id,
                completionAction.DayOfMonth,
                completionAction.IsChecked
            );
            dispatcher.Dispatch(new SetCompletionItemAction(completionAction.Item, completionMarker));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
