using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.ClientPayments.Effects;

public class LoadListEffect : Effect<LoadClientPaymentListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ClientPaymentState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<ClientPaymentState> state,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadClientPaymentListAction action, IDispatcher dispatcher)
    {
        try
        {
            var isLoad = action.IsReload || !action.IsReload && !_state.Value.IsLoaded;
            if (!isLoad)
            {
                return;
            }

            dispatcher.Dispatch(new SetClientPaymentIsListLoadingAction(true));
            var response = await _apiService.ClientPaymentGetListAsync(new GetListRequest
            {
                // Fixes payment pagination by loading the page selected in TPaginator.
                Page = Math.Max(1, _state.Value.SelectedPage)
            });

            if (response != null)
            {
                dispatcher.Dispatch(new SetClientPaymentListItemsAction(response));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetClientPaymentIsListLoadingAction(false));
        }
    }
}
