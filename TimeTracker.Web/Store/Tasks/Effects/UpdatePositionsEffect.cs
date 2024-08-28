using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Dashboard;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Store.Tasks.Effects;

public class UpdatePositionsEffect: Effect<UpdatePositionsAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<UpdatePositionsEffect> _logger;
    private readonly ToastService _toastService;

    public UpdatePositionsEffect(
        ApiService apiService,
        ILogger<UpdatePositionsEffect> logger,
        ToastService toastService
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
            var taskListId = action.Tasks.Select(x => x.TaskList.Id).FirstOrDefault();
            if (taskListId == 0)
            {
                _logger.LogError("Task List Id can not be null");
            }

            var items = action.Tasks.ToDictionary(x => x.TaskId, x => x.PositionIndex);
            await _apiService.TasksUpdatePositionsAsync(new UpdatePositionsRequest()
            {
                TaskListId = taskListId,
                Items = items
            });
        }
        catch (Exception e)
        {
            _toastService.ShowError("Task adding error");
            _logger.LogError(e.Message, e);
        }
    }
}
