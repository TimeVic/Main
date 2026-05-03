using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.MemberPayments.Effects;

public class LoadListEffect: Effect<LoadMemberPaymentListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<MemberPaymentState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<MemberPaymentState> state,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadMemberPaymentListAction action, IDispatcher dispatcher)
    {
        try
        {
            var isLoad = action.IsReload || !action.IsReload && !_state.Value.IsLoaded;
            if (!isLoad)
            {
                return;
            }

            dispatcher.Dispatch(new SetIsListLoading(true));
            var response = await _apiService.MemberPaymentGetListAsync(new GetListRequest()
            {
                WorkspaceId = _authState.Value.Workspace!.Id,
                Page = 1
            });
            dispatcher.Dispatch(new SetListItemsAction(response));
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
