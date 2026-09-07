using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Client;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class ClientsSelect
{
    [Inject]
    private IStringLocalizer<DashboardResource> DashboardLocalizer { get; set; } = default!;

    [Inject]
    public IState<ClientState> _state { get; set; } = default!;

    [Inject]
    public ISecurityManager _securityManager { get; set; } = default!;

    [Inject]
    public IAppModalDialogService _modalDialogService { get; set; } = null!;

    private bool IsCanCreateClient => _securityManager.HasPermission(WorkspacePermission.CreateClient);

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

    private async Task OnAddClient()
    {
        if (!IsCanCreateClient)
        {
            return;
        }

        var result = await _modalDialogService.ShowAsync<AddClientModal>(
            options: new AppModalOptions
            {
                Size = AppModalSize.Small,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            }
        );

        if (result.IsSuccess && result.Data is ClientDto createdClient)
        {
            UpdateList();
            OnClientSelected(createdClient);
        }
    }
}
