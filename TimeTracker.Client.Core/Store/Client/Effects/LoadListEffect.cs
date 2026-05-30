using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Client.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ClientState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<ClientState> state,
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
            var isLoad = action.IsReload || !action.IsReload && !_state.Value.IsLoaded;
            if (!isLoad)
            {
                return;
            }

            if (_authState.Value.IsLoggedIn)
            {
                dispatcher.Dispatch(new SetIsListLoading(true));
                var response = await _apiService.ClientGetListAsync(new GetListRequest()
                {
                    Page = 1
                });
                dispatcher.Dispatch(new SetListItemsAction(response));    
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
