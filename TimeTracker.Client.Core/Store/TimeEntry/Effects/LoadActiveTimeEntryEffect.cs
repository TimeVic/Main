using Fluxor;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.TimeEntry.Effects;

public class LoadActiveTimeEntryEffect : Effect<LoadActiveTimeEntryAction>
{
    private readonly IState<TimeEntryState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadActiveTimeEntryEffect> _logger;

    public LoadActiveTimeEntryEffect(
        IState<TimeEntryState> state,
        IApiService apiService,
        ILogger<LoadActiveTimeEntryEffect> logger
    )
    {
        _state = state;
        _apiService = apiService;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadActiveTimeEntryAction action, IDispatcher dispatcher)
    {
        if (_state.Value.IsTimeEntryProcessing)
        {
            return;
        }

        try
        {
            var response = await _apiService.TimeEntryGetActiveAsync();
            dispatcher.Dispatch(new SetActiveTimeEntryAction(response?.ActiveTimeEntry));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to synchronize the active time entry");
        }
    }
}
