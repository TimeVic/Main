using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.MemberPayments.Effects;

public class AddEffect: Effect<AddMemberPaymentAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<MemberPaymentState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly ToastService _notificationService;

    public AddEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<MemberPaymentState> state,
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

    public override async Task HandleAsync(AddMemberPaymentAction action, IDispatcher dispatcher)
    {
        try
        {
            var payment = await _apiService.MemberPaymentAddAsync(action.Request);
            if (payment != null)
            {
                dispatcher.Dispatch(new LoadMemberPaymentListAction(true));
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
