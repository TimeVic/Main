using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.MemberPayments.Effects;

public class DeleteEffect: Effect<DeleteMemberPaymentAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<MemberPaymentState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly IToastService _notificationService;

    public DeleteEffect(
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
