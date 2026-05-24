using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.WorkspaceMembers.Effects;

public class UpdateEffect: Effect<UpdateMemberAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<WorkspaceMembersState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly IToastService _notificationService;

    public UpdateEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<WorkspaceMembersState> state,
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

    public override async Task HandleAsync(UpdateMemberAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new UpdateRequest
            {
                Access = action.Access,
                ProjectsAccess = action.Projects?
                    .Select(p => new MemberProjectAccessRequest
                    {
                        ProjectId = p.Id,
                        HasAccess = true
                    }).ToList() ?? []
            };
            await _apiService.WorkspaceMemberUpdateAsync(request);
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
