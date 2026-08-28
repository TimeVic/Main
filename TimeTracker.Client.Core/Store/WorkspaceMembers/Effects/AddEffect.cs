using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.WorkspaceMembers.Effects;

public class AddEffect: Effect<AddNewMemberAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<WorkspaceMembersState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<AddEffect> _logger;
    private readonly IToastService _notificationService;

    public AddEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<WorkspaceMembersState> state,
        ILogger<AddEffect> logger,
        IToastService notificationService
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
            dispatcher.Dispatch(new SetIsListLoading(true));
            await _apiService.WorkspaceMemberAddAsync(_authState.Value.Workspace!.Id, action.Email);
            dispatcher.Dispatch(new LoadListAction(true));
            
            _notificationService.ShowInfo("Workspace invitation was sent");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            _notificationService.ShowError(e.Message);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
