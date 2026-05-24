using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.MemberPayments.Effects;

public class AddEffect: Effect<AddMemberPaymentAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<MemberPaymentState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly IToastService _notificationService;

    public AddEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<MemberPaymentState> state,
        ILogger<LoadListEffect> logger,
        IToastService notificationService
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
