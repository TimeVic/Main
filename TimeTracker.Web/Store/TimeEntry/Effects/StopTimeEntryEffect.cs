using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Services.DateTimes;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class StopTimeEntryEffect: Effect<StopActiveTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly UserDateTimeProviderService _dateTimeProviderService;
    private readonly ApiService _apiService;
    private readonly ILogger<StopTimeEntryEffect> _logger;

    public StopTimeEntryEffect(
        ApiService apiService,
        IState<AuthState> authState,
        UserDateTimeProviderService dateTimeProviderService,
        ILogger<StopTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _dateTimeProviderService = dateTimeProviderService;
        _logger = logger;
    }

    public override async Task HandleAsync(StopActiveTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(true));
            await _apiService.TimeEntryStopAsync(new StopRequest()
            {
                WorkspaceId = _authState.Value.Workspace!.Id,
                EndTime = DateTime.UtcNow
            });
            dispatcher.Dispatch(new SetActiveTimeEntryAction(null));
            dispatcher.Dispatch(new SetSelectedPageAction(1));
            dispatcher.Dispatch(new LoadListAction());
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(false));
        }
    }
}
