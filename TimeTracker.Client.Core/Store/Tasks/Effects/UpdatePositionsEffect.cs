using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Dashboard;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Client.Core.Store.Tasks.Effects;

public class UpdatePositionsEffect: Effect<UpdatePositionsAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<UpdatePositionsEffect> _logger;
    private readonly IToastService _toastService;

    public UpdatePositionsEffect(
        IApiService apiService,
        ILogger<UpdatePositionsEffect> logger,
        IToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(UpdatePositionsAction action, IDispatcher dispatcher)
    {
        try
        {
            var taskListId = action.Tasks.Select(x => x.TaskList?.Id ?? Guid.Empty).FirstOrDefault(id => id != Guid.Empty);
            if (taskListId == Guid.Empty)
            {
                _logger.LogError("Task List Id can not be null");
                return;
            }

            var items = action.Tasks.DistinctBy(x => x.Id).ToDictionary(x => x.Id, x => x.PositionIndex);
            await _apiService.TasksUpdatePositionsAsync(new UpdatePositionsRequest()
            {
                TaskListId = taskListId,
                Items = items
            });
        }
        catch (Exception e)
        {
            _toastService.ShowError("Task position updating error");
            _logger.LogError(e.Message, e);
        }
    }
}
