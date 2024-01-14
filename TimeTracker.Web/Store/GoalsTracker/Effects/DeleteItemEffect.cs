using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.GoalsTracker.Effects;

public class DeleteItemEffect: Effect<DeleteTrackerItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<DeleteItemEffect> _logger;
    private readonly ToastService _toastService;

    public DeleteItemEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<DeleteItemEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(DeleteTrackerItemAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.GoalsTrackerDeleteItemAsync(action.Item.Id);
            dispatcher.Dispatch(new DeleteTrackerItemFromListAction(action.Item));
        }
        catch (Exception e)
        {
            await _toastService.ShowError("Goal adding error");
            _logger.LogError(e.Message, e);
        }
    }
}
