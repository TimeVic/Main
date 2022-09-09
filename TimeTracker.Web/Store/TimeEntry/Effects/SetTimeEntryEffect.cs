using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class SetTimeEntryEffect: Effect<SetTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly ILogger<SetTimeEntryEffect> _logger;

    public SetTimeEntryEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<SetTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(SetTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _apiService.TimeEntrySetAsync(new SetRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id
            });
            dispatcher.Dispatch(new SetActiveTimeEntryAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
