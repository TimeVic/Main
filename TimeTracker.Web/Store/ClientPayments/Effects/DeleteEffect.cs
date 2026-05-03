using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Store.ClientPayments.Effects;

public class DeleteEffect : Effect<DeleteClientPaymentAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<DeleteEffect> _logger;
    private readonly ToastService _notificationService;

    public DeleteEffect(
        ApiService apiService,
        ILogger<DeleteEffect> logger,
        ToastService notificationService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(DeleteClientPaymentAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetClientPaymentIsListLoadingAction(true));
            await _apiService.ClientPaymentDeleteAsync(action.ClientPaymentId);
            dispatcher.Dispatch(new RemoveClientPaymentListItemAction(action.ClientPaymentId));
            _notificationService.ShowInfo("Client payment was deleted");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _notificationService.ShowError("Client payment delete error: " + e.Message);
        }
        finally
        {
            dispatcher.Dispatch(new SetClientPaymentIsListLoadingAction(false));
        }
    }
}
