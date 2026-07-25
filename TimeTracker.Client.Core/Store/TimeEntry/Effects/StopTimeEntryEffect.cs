using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Client.Core.Store.TimeEntry.Effects;

public class StopTimeEntryEffect: Effect<StopActiveTimeEntryAction>
{
    private readonly IState<TimeEntryState> _timeEntryState;
    private readonly IApiService _apiService;
    private readonly ILogger<StopTimeEntryEffect> _logger;

    public StopTimeEntryEffect(
        IApiService apiService,
        IState<TimeEntryState> timeEntryState,
        ILogger<StopTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _timeEntryState = timeEntryState;
        _logger = logger;
    }

    public override async Task HandleAsync(StopActiveTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(true));
            var stoppedTimeEntry = await _apiService.TimeEntryStopAsync(new StopRequest()
            {
                EndTime = DateTime.UtcNow
            });
            if (stoppedTimeEntry?.Task != null)
            {
                dispatcher.Dispatch(new UpdateListItemsAction(new[] { stoppedTimeEntry.Task }));
            }

            dispatcher.Dispatch(new SetActiveTimeEntryAction(null));
            ReloadListIfVisible(dispatcher);
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

    private void ReloadListIfVisible(IDispatcher dispatcher)
    {
        if (!_timeEntryState.Value.IsTimeEntryListVisible)
        {
            return;
        }

        dispatcher.Dispatch(new SetSelectedPageAction(1));
        dispatcher.Dispatch(new LoadListAction());
    }
}
