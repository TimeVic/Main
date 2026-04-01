using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Store.List.Currency.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly ApiService _apiService;
    private readonly IState<CurrencyState> _state;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
        IState<CurrencyState> state,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        try
        {
            if (_state.Value.List.Count != 0)
            {
                return;
            }
            dispatcher.Dispatch(new SetIsListLoading(true));
            var response = await _apiService.ListCurrenciesGetAll();
            dispatcher.Dispatch(new SetListItemsAction(response));
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
