using Fluxor;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Payment.Effects;

public class DeleteEffect: Effect<DeletePaymentAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<PaymentState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly NotificationService _notificationService;

    public DeleteEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<PaymentState> state,
        ILogger<LoadListEffect> logger,
        NotificationService notificationService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(DeletePaymentAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.PaymentDeleteAsync(action.PaymentId);
            dispatcher.Dispatch(new RemoveEmptyPaymentListItemAction());
            dispatcher.Dispatch(new RemovePaymentListItemAction(action.PaymentId));
            
            _notificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Info,
                Summary = "Payment was deleted"
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetPaymentIsListLoading(false));
        }
    }
}
