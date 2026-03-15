using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Messaging.Channels.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
        IState<AuthState> authState,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsListLoadingAction(true));
            if (action.IsRefresh)
            {
                dispatcher.Dispatch(new RefreshListAction());
            }

            if (_authState.Value.Workspace != null)
            {
                var response = await _apiService.MessagingChannelGetListAsync(_authState.Value.Workspace.Id);
                if (response is { Items.Count: 0 })
                {
                    await _apiService.MessagingChannelInitAsync(_authState.Value.Workspace.Id);
                }
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
