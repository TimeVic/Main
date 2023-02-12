using Fluxor;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Project.Effects;

public class AddEffect: Effect<SaveEmptyProjectListItemAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ProjectState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly NotificationService _notificationService;

    public AddEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<ProjectState> state,
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

    public override async Task HandleAsync(SaveEmptyProjectListItemAction action, IDispatcher dispatcher)
    {
        try
        {
            if (_state.Value.ItemToAdd == null)
            {
                return;
            }

            await _apiService.ProjectAddAsync(new AddRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                Name = _state.Value.ItemToAdd.Name
            });
            dispatcher.Dispatch(new RemoveEmptyProjectListItemAction());
            dispatcher.Dispatch(new LoadListAction(true));
            
            _notificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Info,
                Summary = "New project was added"
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetProjectIsListLoading(false));
        }
    }
}
