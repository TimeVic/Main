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
    private readonly IState<AuthState> _authState;
    private readonly IState<TimeEntryState> _timeEntryState;
    private readonly IState<ProjectState> _projectState;
    private readonly IApiService _apiService;
    private readonly ILogger<StartTimeEntryEffect> _logger;

    public StartTimeEntryEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<TimeEntryState> timeEntryState,
        IState<ProjectState> projectState,
        ILogger<StartTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _timeEntryState = timeEntryState;
        _projectState = projectState;
        _logger = logger;
    }

    public override async Task HandleAsync(StartTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            var isWasStopped = false;
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(true));
            if (_timeEntryState.Value.HasActiveEntry)
            {
                var stoppedTimeEntry = await _apiService.TimeEntryStopAsync(new StopRequest()
                {
                    EndTime = DateTime.UtcNow
                });
                if (stoppedTimeEntry?.Task != null)
                {
                    dispatcher.Dispatch(new UpdateListItemsAction(new[] { stoppedTimeEntry.Task }));
                }

                isWasStopped = true;
            }

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
            if (isWasStopped)
            {
                dispatcher.Dispatch(new SetSelectedPageAction(1));
                dispatcher.Dispatch(new LoadListAction());
            }
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
