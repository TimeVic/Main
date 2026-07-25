using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Client.Core.Store.TimeEntry.Effects;

public class StartTimeEntryEffect: Effect<StartTimeEntryAction>
{
    private readonly IState<TimeEntryState> _timeEntryState;
    private readonly IState<ProjectState> _projectState;
    private readonly IApiService _apiService;
    private readonly ILogger<StartTimeEntryEffect> _logger;

    public StartTimeEntryEffect(
        IApiService apiService,
        IState<TimeEntryState> timeEntryState,
        IState<ProjectState> projectState,
        ILogger<StartTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _timeEntryState = timeEntryState;
        _projectState = projectState;
        _logger = logger;
    }

    public override async Task HandleAsync(StartTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(true));
            var project = _projectState.Value.List.FirstOrDefault(
                item => item.Id == action.Project?.Id
            );
            if (action.InternalTask != null)
            {
                project = action.InternalTask?.TaskList.Project;
            }
            var response = await _apiService.TimeEntryStartAsync(new StartRequest()
            {
                StartTime = DateTime.UtcNow,
                IsBillable = project != null ? project.IsBillableByDefault : action.IsBillable,
                ProjectId = action.Project?.Id,
                Description = action.Description,
                HourlyRate = action.HourlyRate,
                InternalTaskId = action.InternalTask?.Id
            });
            dispatcher.Dispatch(new SetActiveTimeEntryAction(response));
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
