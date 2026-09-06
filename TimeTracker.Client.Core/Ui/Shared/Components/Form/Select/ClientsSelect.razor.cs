using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Client;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class ClientsSelect
{
    [Inject]
    public IState<ClientState> _state { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = DashboardLocalizer["SelectClient"].Value;

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }

    private void UpdateList()
    {
        _list = _state.Value.List;
        UpdateSelectedItem();
        InvokeAsync(StateHasChanged);
    }

    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }

    private void OnClientSelected(ClientDto? client)
    {
        OnValueChanged(client);
    }
}
