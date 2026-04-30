using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants.Ui;
using TimeTracker.Web.Store.Client;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class ClientsDropDown
{
    [Parameter]
    public InputVariant Variant { get; set; } = InputVariant.Outlined;

    [Inject]
    public IState<ClientState> _state { get; set; }

    private bool _isOpen;

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
        UpdateSelectedItem();
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

    private async Task OnClientSelected(ClientDto? client)
    {
        _isOpen = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        OnValueChanged(client);
    }
}
