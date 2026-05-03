using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Store.ClientPayments.Effects;

public class UpdateEffect : Effect<UpdateClientPaymentAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<UpdateEffect> _logger;
    private readonly ToastService _notificationService;

    public UpdateEffect(
        ApiService apiService,
        ILogger<UpdateEffect> logger,
        ToastService notificationService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(UpdateClientPaymentAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetClientPaymentIsListLoadingAction(true));
            var payment = await _apiService.ClientPaymentUpdateAsync(action.Request);
            if (payment != null)
            {
                dispatcher.Dispatch(new SetClientPaymentListItemAction(payment));
                _notificationService.ShowInfo("Client payment was updated");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _notificationService.ShowError("Client payment update error: " + e.Message);
        }
        finally
        {
            dispatcher.Dispatch(new SetClientPaymentIsListLoadingAction(false));
        }
    }
}
