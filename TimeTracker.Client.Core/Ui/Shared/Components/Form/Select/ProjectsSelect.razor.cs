using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Constants.Ui;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.Client;
using TimeTracker.Client.Core.Store.Permissions;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class ProjectsSelect : BaseSingleSelect<ProjectDto>, IDisposable
{
    private sealed class ProjectClientGroup
    {
        public required Guid ClientId { get; init; }

        public required string Name { get; init; }

        public required IReadOnlyCollection<ProjectDto> Projects { get; init; }
    }

    [Parameter]
    public bool IsShowProjectsWithoutClients { get; set; } = true;

    [Parameter]
    public bool IsGroupByClient { get; set; }

    [Parameter]
    public Guid? ClientId
    {
        get => _clientId;
        set
        {
            if (value != _clientId)
            {
                _clientId = value;
                UpdateList();
            }
        }
    }

    [Inject]
    public IState<ProjectState> _state { get; set; }

    [Inject]
    public IState<ClientState> _clientState { get; set; }

    [Inject]
    public IState<WorkspacePermissionsState> _workspacePermissionsState { get; set; }

    [Inject]
    public ISecurityManager _securityManager { get; set; }

    private Guid? _clientId;
    private bool _isAddClientModalOpened;
    private bool _isAddProjectModalOpened;
    private Guid? _projectClientIdToAdd;

    private bool IsCanCreateClient => _securityManager.HasPermission(WorkspacePermission.CreateClient);
    private bool IsCanCreateProject => _securityManager.HasPermission(WorkspacePermission.CreateProject);
    private bool _shouldGroupByClient => IsGroupByClient && (!_clientId.HasValue || _clientId.Value == Guid.Empty);
    private bool _shouldShowAddFirstClientAction =>
        IsCanCreateClient && _shouldGroupByClient && _clientState.Value.IsLoaded && !_clientState.Value.List.Any();

    private IReadOnlyCollection<ProjectClientGroup> _projectGroups => _shouldGroupByClient
        ? GetClientProjectGroups()
        : new List<ProjectClientGroup>
        {
            new()
            {
                ClientId = Guid.Empty,
                Name = string.Empty,
                Projects = _list.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList()
            }
        };

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = DashboardLocalizer["SelectProject"].Value;

        _state.StateChanged += UpdateList;
        _clientState.StateChanged += UpdateList;
        _workspacePermissionsState.StateChanged += UpdateList;
        UpdateList();
    }

    private void UpdateList(object? sender, EventArgs e)
    {
        UpdateList();
    }

    private void UpdateList()
    {
        _list = _state.Value.List.ToList();
        if (_clientId == Guid.Empty && IsShowProjectsWithoutClients)
        {
            _list = _list.Where(item => item.Client == null).ToList();
        }
        else if (_clientId.HasValue && _clientId.Value != Guid.Empty)
        {
            _list = _list.Where(item => item.Client?.Id == _clientId).ToList();
        }

        if (!IsShowProjectsWithoutClients)
        {
            _list = _list.Where(item => item.Client != null).ToList();
        }

        UpdateSelectedItem();
        InvokeAsync(StateHasChanged);
    }

    private IReadOnlyCollection<ProjectClientGroup> GetClientProjectGroups()
    {
        var groups = _clientState.Value.List
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Select(client => new ProjectClientGroup
            {
                ClientId = client.Id,
                Name = client.Name,
                Projects = _list
                    .Where(project => project.Client?.Id == client.Id)
                    .OrderBy(project => project.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .ToList();

        var projectsWithoutClient = _list
            .Where(project => project.Client == null)
            .OrderBy(project => project.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (IsShowProjectsWithoutClients && projectsWithoutClient.Any())
        {
            groups.Add(new ProjectClientGroup
            {
                ClientId = Guid.Empty,
                Name = DashboardLocalizer["NoClient"].Value,
                Projects = projectsWithoutClient
            });
        }

        return groups;
    }

    private string? GetProjectDescription(ProjectDto project)
    {
        return _shouldGroupByClient ? null : project.Client?.Name;
    }

    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }

    private void OnProjectSelected(ProjectDto? project)
    {
        OnValueChanged(project);
    }

    private Task OnAddFirstClient()
    {
        if (!IsCanCreateClient)
        {
            return Task.CompletedTask;
        }

        _isAddClientModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OnAddProject(Guid clientId)
    {
        if (!IsCanCreateProject)
        {
            return Task.CompletedTask;
        }

        _projectClientIdToAdd = clientId;
        _isAddProjectModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OnAddProjectModalOpenedChanged(bool isOpened)
    {
        _isAddProjectModalOpened = isOpened;
        if (!isOpened)
        {
            _projectClientIdToAdd = null;
        }

        return Task.CompletedTask;
    }

    public new void Dispose()
    {
        _state.StateChanged -= UpdateList;
        _clientState.StateChanged -= UpdateList;
        _workspacePermissionsState.StateChanged -= UpdateList;
        base.Dispose();
    }
}
