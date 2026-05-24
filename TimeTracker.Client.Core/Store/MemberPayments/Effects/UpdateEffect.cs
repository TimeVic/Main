using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.MemberPayments.Effects;

public class UpdateEffect: Effect<UpdateAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly IToastService _notificationService;

    public UpdateEffect(
        IApiService apiService,
        ILogger<LoadListEffect> logger,
        IToastService notificationService
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
            var payment = await _apiService.MemberPaymentUpdateAsync(action.Request);
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
