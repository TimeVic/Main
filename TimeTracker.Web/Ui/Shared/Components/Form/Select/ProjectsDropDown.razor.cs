using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class ProjectsDropDown: IDisposable
{   
    [Parameter]
    public bool ShowProjectsWithoutClients { get; set; } = true;
    
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
        UpdateSelectedItem();
        if (_clientId == Guid.Empty && ShowProjectsWithoutClients)
        {
            _list = _list.Where(item => item.Client == null).ToList();
            return;
        }
        if (!_clientId.HasValue || _clientId.Value == Guid.Empty)
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

    public void Dispose()
    {
        _state.StateChanged -= UpdateList;
    }
}
