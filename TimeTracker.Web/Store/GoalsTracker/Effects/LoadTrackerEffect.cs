using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.GoalsTracker.Effects;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker.Effects;

public class LoadTrackerEffect: Effect<LoadTrackerAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<GoalsTrackerState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadTrackerEffect> _logger;

    public LoadTrackerEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<GoalsTrackerState> state,
        ILogger<LoadTrackerEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadTrackerAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetIsListLoadingAction(true));
        try
        {
            dispatcher.Dispatch(new SetTrackerAction(new GoalsTrackerDto()
            {
                Year = action.Year,
                Month = action.Month,
                Items = new List<GoalsTrackerItemDto>()
                {
                    new() { Id = 1, Name = $"Test1 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 2, Name = $"Test2 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 3, Name = $"Test3 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 4, Name = $"Test4 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 5, Name = $"Test5 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 6, Name = $"Test6 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 7, Name = $"Test7 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 8, Name = $"Test8 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 9, Name = $"Test9 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 10, Name = $"Test10 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                    new() { Id = 11, Name = $"Test11 - {action.Year} - {action.Month}", NumberOfTimes = 2, CompletionMarkers = new List<GoalsTrackerCompletionMarkerDto>()},
                }
            }));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoadingAction(false));
        }
    }
}
