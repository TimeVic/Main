using Fluxor;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Payment.Effects;

public class AddEffect: Effect<SaveEmptyPaymentListItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<PaymentState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly NotificationService _notificationService;

    public AddEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<PaymentState> state,
        ILogger<LoadListEffect> logger,
        NotificationService notificationService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(SaveEmptyPaymentListItemAction action, IDispatcher dispatcher)
    {
        try
        {
            if (_state.Value.ItemToAdd == null)
            {
                return;
            }

            await _apiService.PaymentAddAsync(new AddRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                ClientId = _state.Value.ItemToAdd.Client.Id,
                ProjectId = _state.Value.ItemToAdd.Project?.Id,
                Amount = _state.Value.ItemToAdd.Amount,
                Description = _state.Value.ItemToAdd.Description,
                PaymentTime = _state.Value.ItemToAdd.PaymentTime
            });
            dispatcher.Dispatch(new RemoveEmptyPaymentListItemAction());
            dispatcher.Dispatch(new LoadPaymentListAction(true));
            
            _notificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Info,
                Summary = "New Payment was added"
            });
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
