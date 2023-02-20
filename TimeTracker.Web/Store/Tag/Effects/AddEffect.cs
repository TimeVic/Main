using Fluxor;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Tag.Effects;

public class AddEffect: Effect<SaveEmptyListItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<TagState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly NotificationService _notificationService;

    public AddEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<TagState> state,
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

    public override async Task HandleAsync(SaveEmptyListItemAction action, IDispatcher dispatcher)
    {
        try
        {
            if (_state.Value.ItemToAdd == null)
            {
                return;
            }

            await _apiService.TagAddAsync(new AddRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                Name = _state.Value.ItemToAdd.Name,
                Color = _state.Value.ItemToAdd.Color,
            });
            dispatcher.Dispatch(new RemoveEmptyListItemAction());
            dispatcher.Dispatch(new LoadListAction(true));
            
            _notificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Info,
                Summary = "New tag was added"
            });
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
