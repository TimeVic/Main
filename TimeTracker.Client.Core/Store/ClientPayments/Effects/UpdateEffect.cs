using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.ClientPayments.Effects;

public class UpdateEffect : Effect<UpdateClientPaymentAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<UpdateEffect> _logger;
    private readonly IToastService _notificationService;

    public UpdateEffect(
        IApiService apiService,
        ILogger<UpdateEffect> logger,
        IToastService notificationService
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
