using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.MemberPayments.Effects;

public class DeleteEffect: Effect<DeleteMemberPaymentAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<MemberPaymentState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly ToastService _notificationService;

    public DeleteEffect(
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

    public override async Task HandleAsync(DeleteMemberPaymentAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.MemberPaymentDeleteAsync(action.MemberPaymentId);
            dispatcher.Dispatch(new RemoveMemberPaymentListItemAction(action.MemberPaymentId));
            
            _notificationService.ShowInfo("Payment was deleted");
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
