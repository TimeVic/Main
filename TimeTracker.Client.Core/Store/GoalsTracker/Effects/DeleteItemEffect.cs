using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.GoalsTracker.Effects;

public class DeleteItemEffect: Effect<DeleteTrackerItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<DeleteItemEffect> _logger;
    private readonly IToastService _toastService;

    public DeleteItemEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<DeleteItemEffect> logger,
        IToastService toastService
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
            _toastService.ShowError("Goal adding error");
            _logger.LogError(e.Message, e);
        }
    }
}
