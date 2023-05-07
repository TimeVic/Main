using Fluxor;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Payment.Effects;

public class UpdateEffect: Effect<SavePaymentListItemAction>
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

    public override async Task HandleAsync(SavePaymentListItemAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.PaymentUpdateAsync(new UpdateRequest()
            {
                PaymentId = action.Payment.Id,
                ClientId = action.Payment.Client.Id,
                ProjectId = action.Payment.Project?.Id,
                Amount = action.Payment.Amount,
                Description = action.Payment.Description,
                PaymentTime = action.Payment.PaymentTime
            });
            dispatcher.Dispatch(new LoadPaymentListAction(true));
            await _notificationService.ShowInfo("Payment was updated");
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
