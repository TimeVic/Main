using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Shared.Components.Form.Select;

public partial class ProjectsDropDown
{   
    [Parameter]
    public bool ShowProjectsWithoutClients { get; set; } = true;
    
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

    [Inject]
    public IState<ProjectState> _state { get; set; }
    
    private long? _clientId;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = "Select project";

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
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
    
    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }
}
