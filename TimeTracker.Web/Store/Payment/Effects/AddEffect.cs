using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Payment.Effects;

public class AddEffect: Effect<AddPaymentAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<PaymentState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly ToastService _notificationService;

    public AddEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<PaymentState> state,
        ILogger<LoadListEffect> logger,
        ToastService notificationService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(AddPaymentAction action, IDispatcher dispatcher)
    {
        try
        {
            action.Request.WorkspaceId = _authState.Value.Workspace!.Id;
            var payment = await _apiService.PaymentAddAsync(action.Request);
            if (payment != null)
            {
                dispatcher.Dispatch(new LoadPaymentListAction(true));
            }
            _notificationService.ShowInfo("Payment has been added");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _notificationService.ShowError("Payment adding error: " + e.Message);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
