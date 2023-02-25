using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Store.TasksList.Effects;

public class ArchiveEffect: Effect<ArchiveTaskListAction>
{
    private readonly IApiService _apiService;
    private readonly IState<TasksListState> _taskListState;
    private readonly IState<ProjectState> _projectState;
    private readonly ILogger<ArchiveEffect> _logger;
    private readonly NavigationManager _navigationManager;

    public ArchiveEffect(
        IApiService apiService,
        IState<TasksListState> taskListState,
        IState<ProjectState> projectState,
        ILogger<ArchiveEffect> logger,
        NavigationManager navigationManager
    )
    {
        _apiService = apiService;
        _taskListState = taskListState;
        _projectState = projectState;
        _logger = logger;
        _navigationManager = navigationManager;
    }

    public override async Task HandleAsync(ArchiveTaskListAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.TaskListArchiveAsync(action.TaskList.Id);
            dispatcher.Dispatch(new RemoveListItemsAction(action.TaskList.Id));
            var navigateToProjectsClient = _projectState.Value.List.FirstOrDefault(
                item => item.Id == action.TaskList.Project.Id
            );
            var firstTaskList = _taskListState.Value.List.FirstOrDefault(
                item => item.Project.Id == action.TaskList.Project.Id
            );
            _navigationManager.NavigateTo(
                string.Format(
                    SiteUrl.Dashboard_Tasks,
                    navigateToProjectsClient?.Client?.Id.ToString() ?? "0",
                    firstTaskList?.Id.ToString() ?? ""
                )    
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
