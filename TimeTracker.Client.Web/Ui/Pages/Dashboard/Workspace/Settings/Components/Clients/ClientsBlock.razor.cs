using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Client;
using TimeTracker.Client.Core.Store.Project;

using TimeTracker.Client.Web.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Clients;

public partial class ClientsBlock
{
    [Inject]
    private IState<ClientState> _clientState { get; set; } = default!;

    [Inject]
    private IState<ProjectState> _projectState { get; set; } = default!;

    [Inject]
    private IModalDialogProviderService _modalDialogService { get; set; } = default!;

    private string _searchQuery { get; set; } = string.Empty;

    private async Task OnAdd()
    {
        await _modalDialogService.ShowAddClientModal();
    }

    private async Task OnAddProjectTop()
    {
        await _modalDialogService.ShowAddProjectModal();
    }

    private async Task OnEditClient(ClientDto client)
    {
        await _modalDialogService.ShowUpdateClientModal(client);
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

    private async Task OnAddProject(ClientDto? client)
    {
        await _modalDialogService.ShowAddProjectModal(client?.Id);
    }

    private async Task OnEditProject(ProjectDto project)
    {
        await _modalDialogService.ShowUpdateProjectModal(project);
    }
}
