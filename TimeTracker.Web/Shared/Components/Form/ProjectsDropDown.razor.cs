using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class ProjectsDropDown
{   
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public string? Label { get; set; }
    
    [Parameter]
    public bool Clearable { get; set; } = true;
    
    [Parameter]
    public bool ShowProjectsWithoutClients { get; set; } = true;
    
    [Parameter]
    public long Value
    {
        get
        {
            long.TryParse(_selectedId, out var id);
            return id;
        }
        set
        {
            if (value.ToString() != _selectedId)
            {
                _selectedId = value.ToString();
                UpdateSelectedItem();
            }
        }
    }
    
    [Parameter]
    public EventCallback<ProjectDto> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select project";
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public long? ClientId
    {
        get => _clientId;
        set
        {
            if (value != _clientId)
            {
                _clientId = value;
                UpdateList();
                UpdateSelectedItem();
            }
        }
    }

    [Parameter]
    public bool FullWidth { get; set; } = false;
    
    [Inject]
    public ILogger<ProjectDto> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> _state { get; set; }
    
    private ProjectDto? _selectedItem = null;
    private ICollection<ProjectDto> _list = new List<ProjectDto>();
    private string? _selectedId = null;
    private long? _clientId;
    public string? _placeholder => _selectedItem is null ? Placeholder : null;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }

    private void OnClear()
    {
        if (string.IsNullOrEmpty(_selectedId))
            return;
        _selectedId = null;
    }
    
    private void OnValueChanged(ProjectDto? project)
    {
        if (_selectedItem?.Id != project?.Id)
        {
            UpdateSelectedItem();
            SelectedItemChanged.InvokeAsync(_selectedItem);
        }
    }

    private void UpdateList()
    {
        _list = _state.Value.List;
        if (_clientId == 0 && ShowProjectsWithoutClients)
        {
            _list = _list.Where(item => item.Client == null).ToList();
            return;
        }
        if (!_clientId.HasValue)
        {
            return;
        }
        _list = _list.Where(item => item.Client?.Id == _clientId).ToList();
    }
    
    private void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }
}
