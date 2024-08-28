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
        get => _selectedId;
        set
        {
            if (_selectedId != value)
            {
                _selectedId = value;
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
            }
        }
    }

    [Inject]
    public ILogger<ProjectDto> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> _state { get; set; }
    
    private ProjectDto? _selectedItem => _list.FirstOrDefault(item => item.Id == _selectedId);
    private ICollection<ProjectDto> _list = new List<ProjectDto>();
    private long _selectedId = 0;
    private long? _clientId;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }

    private void OnValueChanged(string? project)
    {
        long.TryParse(project, out _selectedId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
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
}
