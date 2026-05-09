using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.ClientPayments.Effects;

public class AddEffect : Effect<AddClientPaymentAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<AddEffect> _logger;
    private readonly ToastService _notificationService;

    public AddEffect(
        ApiService apiService,
        IState<AuthState> authState,
        ILogger<AddEffect> logger,
        ToastService notificationService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(AddClientPaymentAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetClientPaymentIsListLoadingAction(true));
            var payment = await _apiService.ClientPaymentAddAsync(action.Request);
            if (payment != null)
            {
                dispatcher.Dispatch(new LoadClientPaymentListAction(true));
            }

            _notificationService.ShowInfo("Client payment has been added");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _notificationService.ShowError("Client payment adding error: " + e.Message);
        }
        finally
        {
            dispatcher.Dispatch(new SetClientPaymentIsListLoadingAction(false));
        }
    }
}
