using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.GoalsTracker.Effects;
using TimeTracker.Client.Core.Store.GoalsTracker;

namespace TimeTracker.Client.Core.Store.GoalsTracker.Effects;

public class SetCompletionEffect: Effect<SetItemCompletionAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<SetCompletionEffect> _logger;

    public SetCompletionEffect(
        IApiService apiService,
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
