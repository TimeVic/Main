using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Tasks.Effects;

public class AddSubTaskEffect : Effect<AddSubTaskAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<AddSubTaskEffect> _logger;
    private readonly IToastService _toastService;

    public AddSubTaskEffect(
        IApiService apiService,
        ILogger<AddSubTaskEffect> logger,
        IToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(AddSubTaskAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _apiService.TaskSubTaskAddAsync(new AddRequest
            {
                TaskId = action.TaskId,
                Title = action.Title
            });

            if (response != null)
            {
                dispatcher.Dispatch(new UpdateTaskSubTasksCountsAction(action.TaskId, 1, 0));
                action.OnSuccess?.Invoke(response);
            }
        }
        catch (Exception e)
        {
            _toastService.ShowError("Failed to add subtask");
            _logger.LogError(e, "Failed to add subtask");
        }
    }
}

public class UpdateSubTaskEffect : Effect<UpdateSubTaskAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<UpdateSubTaskEffect> _logger;
    private readonly IToastService _toastService;

    public UpdateSubTaskEffect(
        IApiService apiService,
        ILogger<UpdateSubTaskEffect> logger,
        IToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(UpdateSubTaskAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _apiService.TaskSubTaskUpdateAsync(new UpdateRequest
            {
                SubTaskId = action.SubTaskId,
                Title = action.Title,
                IsCompleted = action.IsCompleted
            });

            if (response != null)
            {
                var completedDelta = action.IsCompleted ? 1 : -1;
                dispatcher.Dispatch(new UpdateTaskSubTasksCountsAction(action.TaskId, 0, completedDelta));
                action.OnSuccess?.Invoke(response);
            }
        }
        catch (Exception e)
        {
            _toastService.ShowError("Failed to update subtask");
            _logger.LogError(e, "Failed to update subtask");
        }
    }
}

public class DeleteSubTaskEffect : Effect<DeleteSubTaskAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<DeleteSubTaskEffect> _logger;
    private readonly IToastService _toastService;

    public DeleteSubTaskEffect(
        IApiService apiService,
        ILogger<DeleteSubTaskEffect> logger,
        IToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(DeleteSubTaskAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.TaskSubTaskDeleteAsync(action.SubTaskId);
            var completedDelta = action.WasCompleted ? -1 : 0;
            dispatcher.Dispatch(new UpdateTaskSubTasksCountsAction(action.TaskId, -1, completedDelta));
            action.OnSuccess?.Invoke();
        }
        catch (Exception e)
        {
            _toastService.ShowError("Failed to delete subtask");
            _logger.LogError(e, "Failed to delete subtask");
        }
    }
}
