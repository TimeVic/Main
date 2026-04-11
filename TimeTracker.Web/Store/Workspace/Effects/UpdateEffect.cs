using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;

namespace TimeTracker.Web.Store.Workspace.Effects;

public class UpdateEffect: Effect<UpdateWorkspaceAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<UpdateEffect> _logger;
    private readonly IState<AuthState> _authState;
    private readonly ToastService _toastService;

    public UpdateEffect(
        ApiService apiService,
        ILogger<UpdateEffect> logger,
        IState<AuthState> authState,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _authState = authState;
        _toastService = toastService;
    }

    public override async Task HandleAsync(UpdateWorkspaceAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsListLoading(true));

            var response = await _apiService.WorkspaceUpdateAsync(action.Model);
            if (response != null)
            {
                dispatcher.Dispatch(new SetListItemAction(response));
                dispatcher.Dispatch(new SetWorkspaceAction(response));
                dispatcher.Dispatch(new PersistDataAction());
                _toastService.ShowInfo($"Workspace updated.");
            }
        }
        catch (Exception e)
        {
            _toastService.ShowError("Workspace saving error");
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
