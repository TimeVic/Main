using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Tag.Effects;

public class DeleteEffect: Effect<DeleteItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<TagState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly ToastService _notificationService;

    public DeleteEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<TagState> state,
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

    public override async Task HandleAsync(DeleteItemAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsSavingAction(true));
            await _apiService.TagDeleteAsync(action.Tag.Id);
            dispatcher.Dispatch(new DeleteListItemAction(action.Tag.Id));
            
            _notificationService.ShowInfo("Tag was deleted");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _notificationService.ShowError("Tag deleting error");
        }
        finally
        {
            dispatcher.Dispatch(new SetIsSavingAction(false));
        }
    }
}
