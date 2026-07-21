using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.TasksList.Effects;

public class LoadDropDownListEffect : Effect<LoadDropDownListAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<LoadDropDownListEffect> _logger;

    public LoadDropDownListEffect(
        IApiService apiService,
        ILogger<LoadDropDownListEffect> logger
    )
    {
        _apiService = apiService;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadDropDownListAction action, IDispatcher dispatcher)
    {
        try
        {
            if (!action.ProjectId.HasValue)
            {
                dispatcher.Dispatch(new SetDropDownListItemsAction(
                    new GetListResponse(new List<TaskListForListDto>(), 0),
                    null
                ));
                return;
            }

            // A project-specific selector must not replace the complete task-list navigation tree.
            var response = await _apiService.TaskListGetListAsync(new GetListRequest
            {
                ProjectId = action.ProjectId
            });
            dispatcher.Dispatch(new SetDropDownListItemsAction(
                response ?? new GetListResponse(new List<TaskListForListDto>(), 0),
                action.ProjectId
            ));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to load task lists for the task-list selector.");
        }
    }
}
