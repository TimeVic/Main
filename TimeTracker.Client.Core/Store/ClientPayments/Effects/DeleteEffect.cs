using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.ClientPayments.Effects;

public class DeleteEffect : Effect<DeleteClientPaymentAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<DeleteEffect> _logger;
    private readonly IToastService _notificationService;

    public DeleteEffect(
        IApiService apiService,
        ILogger<DeleteEffect> logger,
        IToastService notificationService
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
