using Fluxor;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.WorkspaceMemberships.Effects;

public class AddEffect: Effect<AddNewMemberAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<WorkspaceMembershipsState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly NotificationService _notificationService;

    public AddEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<WorkspaceMembershipsState> state,
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

    public override async Task HandleAsync(AddNewMemberAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.WorkspaceMembershipAddAsync(_authState.Value.Workspace.Id, action.Email);
            dispatcher.Dispatch(new LoadListAction(true));
            
            _notificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Info,
                Summary = "New member was added"
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
