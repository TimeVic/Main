using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.Client;
using TimeTracker.Web.Store.TasksList;
using TimeTracker.Web.Store.Ui;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared;

public partial class NavMenu
{
    [Inject]
    protected IState<ClientState> ClientState { get; set; }
    
    [Inject]
    protected NavigationManager _navigationManager { get; set; }
    
    [Inject]
    protected ISecurityManager _securityManager { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    [Inject]
    public IState<UiState> _uiState { get; set; }

    public ICollection<ClientDto> Clients => ClientState.Value.List;

    private string GetTasksListUrl(ProjectDto project)
    {
        var selectedTasksList = _tasksListState.Value.List
            .FirstOrDefault(item => item?.Project.Id == project.Id);
        return string.Format(SiteUrl.Dashboard_Tasks, selectedTasksList?.Id.ToString() ?? string.Empty);
    }

    private IEnumerable<ProjectDto> GetClientProjects(ClientDto? client = null)
    {
        var projects = _securityManager.GetSharedProjects();
        return projects.Where(
            item => item.Client?.Id == client?.Id
            || (client == null && item.Client == null)
        );
    }
}
