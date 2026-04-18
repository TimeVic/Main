using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.WorkspaceMemberships.Effects;

public class UpdateEffect: Effect<UpdateMemberAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<WorkspaceMembershipsState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly ToastService _notificationService;

    public UpdateEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<WorkspaceMembershipsState> state,
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

    public override async Task HandleAsync(UpdateMemberAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new UpdateRequest
            {
                Access = action.Access,
                ProjectsAccess = action.Projects?
                    .Select(p => new MembershipProjectAccessRequest
                    {
                        ProjectId = p.Id,
                        HasAccess = true
                    }).ToList() ?? []
            };
            await _apiService.WorkspaceMembershipUpdateAsync(request);
            dispatcher.Dispatch(new LoadListAction(true));
            _notificationService.ShowInfo("The member was updated");
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
