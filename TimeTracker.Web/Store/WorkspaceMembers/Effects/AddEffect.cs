using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.WorkspaceMembers.Effects;

public class AddEffect: Effect<AddNewMemberAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<WorkspaceMembersState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly ToastService _notificationService;

    public AddEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<WorkspaceMembersState> state,
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
            _logger.LogError(e.Message, e);
            _notificationService.ShowError("Member invitation error");
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
