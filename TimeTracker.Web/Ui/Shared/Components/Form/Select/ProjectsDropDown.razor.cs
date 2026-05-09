using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants.Ui;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.Client;
using TimeTracker.Web.Store.Permissions;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class ProjectsDropDown: IDisposable
{
    private sealed class ProjectClientGroup
    {
        public required Guid ClientId { get; init; }

        public required string Name { get; init; }

        public required IReadOnlyCollection<ProjectDto> Projects { get; init; }
    }

    [Parameter]
    public InputVariant Variant { get; set; } = InputVariant.Outlined;

    [Parameter]
    public Size Size { get; set; } = Size.Medium;
    
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
    private bool _isOpen;
    private bool _isAddClientModalOpened;
    private bool _isAddProjectModalOpened;
    private Guid? _projectClientIdToAdd;
    private readonly ProjectDto _addFirstClientActionItem = new()
    {
        Id = Guid.NewGuid(),
        Name = "Add first client"
    };
    private readonly Dictionary<Guid, ProjectDto> _addProjectActionItems = new();
    private readonly Dictionary<string, ProjectDto> _groupHeaderItems = new();
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
        Placeholder = "Select project";

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
                Name = "No client",
                Projects = projectsWithoutClient
            });
        }

        return groups;
    }

    private string? GetProjectDescription(ProjectDto project)
    {
        return _shouldGroupByClient ? null : project.Client?.Name;
    }

    private ProjectDto GetGroupHeaderItem(string key)
    {
        if (_groupHeaderItems.TryGetValue(key, out var item))
        {
            return item;
        }

        item = new ProjectDto
        {
            Id = Guid.NewGuid(),
            Name = key
        };

        _groupHeaderItems[key] = item;
        return item;
    }

    private ProjectDto GetAddProjectActionItem(Guid clientId)
    {
        if (_addProjectActionItems.TryGetValue(clientId, out var item))
        {
            return item;
        }

        item = new ProjectDto
        {
            Id = Guid.NewGuid(),
            Name = "Add project"
        };

        _addProjectActionItems[clientId] = item;
        return item;
    }

    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }

    private Task OnOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        return Task.CompletedTask;
    }

    private async Task OnProjectSelected(ProjectDto? project)
    {
        _isOpen = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        OnValueChanged(project);
    }

    private async Task OnSelectValueChanged(ProjectDto? project)
    {
        if (project?.Id == _addFirstClientActionItem.Id)
        {
            await OnAddFirstClient();
            await InvokeAsync(StateHasChanged);
            return;
        }

        var addProjectAction = _addProjectActionItems.FirstOrDefault(item => item.Value.Id == project?.Id);
        if (addProjectAction.Value != null)
        {
            await OnAddProject(addProjectAction.Key);
            await InvokeAsync(StateHasChanged);
            return;
        }

        OnValueChanged(project);
    }

    private Task OnAddFirstClient()
    {
        if (!IsCanCreateClient)
        {
            return Task.CompletedTask;
        }

        _isOpen = false;
        _isAddClientModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OnAddProject(Guid clientId)
    {
        if (!IsCanCreateProject)
        {
            return Task.CompletedTask;
        }

        _isOpen = false;
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
