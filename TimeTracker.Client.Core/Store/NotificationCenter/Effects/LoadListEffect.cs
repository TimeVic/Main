using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.NotificationCenter.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<NotificationCenterState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<NotificationCenterState> state,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsListLoadingAction(true));
            var page = _state.Value.NextPage;
            if (action.IsRefresh)
            {
                dispatcher.Dispatch(new RefreshListAction());
                page = 1;
            }

            if (_authState.Value.Workspace != null)
            {
                var response = await _apiService.NotificationCenterGetList(
                    _authState.Value.Workspace.Id,
                    page
                );
                dispatcher.Dispatch(new SetListAction(response));
            }
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
