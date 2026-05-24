using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Client;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class ClientsBlock
{
    [Inject] 
    private IState<ClientState> _clientState { get; set; }
    
    [Inject] 
    private IState<ProjectState> _projectState { get; set; }

    private bool _isAddClientModalOpened { get; set; }
    private bool _isAddProjectModalOpened { get; set; }
    private Guid? _initialProjectClientId { get; set; }
    private ProjectDto? _projectToUpdate { get; set; }

    private Task OnAdd()
    {
        _isAddClientModalOpened = true;
        return Task.CompletedTask;
    }

    private IReadOnlyCollection<ProjectDto> GetProjectsByClient(ClientDto client)
    {
        return _projectState.Value.List
            .Where(project => project.Client?.Id == client.Id)
            .OrderBy(project => project.Name)
            .ToList();
    }

    private IReadOnlyCollection<ProjectDto> GetProjectsWithoutClient()
    {
        return _projectState.Value.List
            .Where(project => project.Client == null)
            .OrderBy(project => project.Name)
            .ToList();
    }

    private Task OnAddProject(ClientDto? client)
    {
        _initialProjectClientId = client?.Id;
        _isAddProjectModalOpened = true;
        return Task.CompletedTask;
    }
    
    private Task OnEditProject(ProjectDto project)
    {
        _projectToUpdate = project;
        return Task.CompletedTask;
    }
    
    private Task OnAddProjectModalOpenedChanged(bool isOpened)
    {
        _isAddProjectModalOpened = isOpened;
        if (!isOpened)
        {
            _initialProjectClientId = null;
        }

        return Task.CompletedTask;
    }
}
