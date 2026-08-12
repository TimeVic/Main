using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Client;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Clients;

public partial class ClientsBlock
{
    [Inject]
    private IState<ClientState> _clientState { get; set; } = default!;

    [Inject]
    private IState<ProjectState> _projectState { get; set; } = default!;

    private bool _isAddClientModalOpened { get; set; }
    private bool _isAddProjectModalOpened { get; set; }
    private Guid? _initialProjectClientId { get; set; }
    private ClientDto? _clientToUpdate { get; set; }
    private ProjectDto? _projectToUpdate { get; set; }
    private string _searchQuery { get; set; } = string.Empty;

    private Task OnAdd()
    {
        _isAddClientModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OnAddProjectTop()
    {
        _initialProjectClientId = null;
        _isAddProjectModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OnEditClient(ClientDto client)
    {
        _clientToUpdate = client;
        return Task.CompletedTask;
    }

    private IEnumerable<ClientDto> GetFilteredClients()
    {
        var clients = _clientState.Value.List.OrderBy(client => client.Name).AsEnumerable();
        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            return clients;
        }

        var query = _searchQuery.Trim();
        return clients.Where(c =>
            c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            GetProjectsByClient(c).Any()
        );
    }

    private IReadOnlyCollection<ProjectDto> GetProjectsByClient(ClientDto client)
    {
        var projects = _projectState.Value.List
            .Where(project => project.Client?.Id == client.Id)
            .OrderBy(project => project.Name);

        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            return projects.ToList();
        }

        var query = _searchQuery.Trim();
        if (client.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return projects.ToList();
        }

        return projects.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private IReadOnlyCollection<ProjectDto> GetProjectsWithoutClient()
    {
        var projects = _projectState.Value.List
            .Where(project => project.Client == null)
            .OrderBy(project => project.Name);

        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            return projects.ToList();
        }

        var query = _searchQuery.Trim();
        return projects.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
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
