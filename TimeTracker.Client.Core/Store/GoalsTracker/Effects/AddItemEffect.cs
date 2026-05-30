using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.GoalsTracker.Effects;

public class CreateItemEffect: Effect<CreateTrackerItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<CreateItemEffect> _logger;
    private readonly IToastService _toastService;

    public CreateItemEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<CreateItemEffect> logger,
        IToastService toastService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(CreateTrackerItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var tracker = await _apiService.GoalsTrackerCreateItemAsync(_state.Value.CurrentTracker.Id, action.Name, action.NumberOfTimes);
            dispatcher.Dispatch(new SetGoalsTrackerItemAction(tracker));
        }
        catch (Exception e)
        {
            _toastService.ShowError("Goal adding error");
            _logger.LogError(e.Message, e);
        }
    }
}
