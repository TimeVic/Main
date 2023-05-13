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

public class UpdateListItemEffect: Effect<UpdateListItemAction>
{
    private readonly IState<TasksState> _state;
    private readonly IState<TasksListState> _tasksListState;
    private readonly ApiService _apiService;
    private readonly ILogger<UpdateListItemEffect> _logger;
    private readonly ToastService _toastService;

    public UpdateListItemEffect(
        ApiService apiService,
        IState<TasksState> state,
        IState<TasksListState> tasksListState,
        ILogger<UpdateListItemEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _state = state;
        _tasksListState = tasksListState;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(UpdateListItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _apiService.TasksUpdateAsync(action.UpdateRequest);
            dispatcher.Dispatch(new SetListItemAction(response));
            dispatcher.Dispatch(new SetTasksListItemAction(response));
        }
        catch (Exception e)
        {
            await _toastService.ShowError("Task adding error");
            _logger.LogError(e.Message, e);
        }
    }
}
