using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Payment.Effects;

public class UpdateEffect: Effect<UpdateAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly ToastService _notificationService;

    public UpdateEffect(
        ApiService apiService,
        ILogger<LoadListEffect> logger,
        ToastService notificationService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(UpdateAction action, IDispatcher dispatcher)
    {
        try
        {
            var payment = await _apiService.PaymentUpdateAsync(action.Request);
            if (payment != null)
            {
                dispatcher.Dispatch(new SetListItemAction(payment));
                _notificationService.ShowInfo("Payment was updated");
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
