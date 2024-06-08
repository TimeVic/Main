using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.GoalsTracker.Effects;

public class CreateItemEffect: Effect<CreateTrackerItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<CreateItemEffect> _logger;
    private readonly ToastService _toastService;

    public CreateItemEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<CreateItemEffect> logger,
        ToastService toastService
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
            await _toastService.ShowError("Goal adding error");
            _logger.LogError(e.Message, e);
        }
    }
}
