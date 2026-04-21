using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Tag.Effects;

public class AddEffect : Effect<AddAction>
{
    private readonly ApiService _apiService;
    private readonly IState<AuthState> _authState;
    private readonly ILogger<AddEffect> _logger;
    private readonly ToastService _toastService;

    public AddEffect(
        ApiService apiService,
        IState<AuthState> authState,
        ILogger<AddEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(AddAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsSavingAction(true));
            action.Request.WorkspaceId = _authState.Value.Workspace!.Id;

            var response = await _apiService.TagAddAsync(action.Request);
            if (response != null)
            {
                dispatcher.Dispatch(new SetListItemAction(response));
                _toastService.ShowInfo("Tag has been added");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _toastService.ShowError("Tag adding error");
        }
        finally
        {
            dispatcher.Dispatch(new SetIsSavingAction(false));
        }
    }
}
