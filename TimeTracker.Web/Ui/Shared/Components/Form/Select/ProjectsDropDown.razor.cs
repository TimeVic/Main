using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants.Ui;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class ProjectsDropDown: IDisposable
{
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
    
    private Guid? _clientId;
    private bool _isOpen;
    private readonly Dictionary<ProjectClientGroupKey, ProjectDto> _groupHeaderItems = new();
    private bool _shouldGroupByClient => IsGroupByClient && (!_clientId.HasValue || _clientId.Value == Guid.Empty);
    private IEnumerable<IGrouping<ProjectClientGroupKey, ProjectDto>> _projectGroups => _shouldGroupByClient
        ? _list
            .OrderBy(GetClientSortName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .GroupBy(GetClientGroupKey)
        : _list.GroupBy(_ => ProjectClientGroupKey.Empty);
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = "Select project";

        _state.StateChanged += UpdateList;
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

    private static ProjectClientGroupKey GetClientGroupKey(ProjectDto project)
    {
        return new ProjectClientGroupKey(
            project.Client?.Id ?? Guid.Empty,
            string.IsNullOrWhiteSpace(project.Client?.Name) ? "No client" : project.Client.Name
        );
    }

    private static string GetClientSortName(ProjectDto project)
    {
        return string.IsNullOrWhiteSpace(project.Client?.Name)
            ? "zzzzzzzz-no-client"
            : project.Client.Name;
    }

    private string? GetProjectDescription(ProjectDto project)
    {
        return _shouldGroupByClient ? null : project.Client?.Name;
    }

    private ProjectDto GetGroupHeaderItem(ProjectClientGroupKey groupKey)
    {
        if (_groupHeaderItems.TryGetValue(groupKey, out var item))
        {
            return item;
        }

        item = new ProjectDto
        {
            Id = Guid.NewGuid(),
            Name = groupKey.Name
        };

        _groupHeaderItems[groupKey] = item;
        return item;
    }

    private readonly record struct ProjectClientGroupKey(Guid Id, string Name)
    {
        public static ProjectClientGroupKey Empty { get; } = new(Guid.Empty, string.Empty);
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

    public new void Dispose()
    {
        _state.StateChanged -= UpdateList;
        base.Dispose();
    }
}
