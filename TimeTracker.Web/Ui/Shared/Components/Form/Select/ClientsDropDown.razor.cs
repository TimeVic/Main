using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.Client;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class ClientsDropDown
{
    [Inject]
    public IState<ClientState> _state { get; set; }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = "Select client";

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void UpdateList()
    {
        _list = _state.Value.List;
    }
    
    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }
}
