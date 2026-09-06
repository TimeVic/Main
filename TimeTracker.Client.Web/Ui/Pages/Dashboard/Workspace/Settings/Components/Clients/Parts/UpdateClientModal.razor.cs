using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Client;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Clients.Parts;

public partial class UpdateClientModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public required ClientDto Client { get; set; }

    [Inject]
    public virtual IState<ClientState> _state { get; set; } = default!;

    private ClientDto model = new();
    private EditForm _form = default!;

    protected override Task OnParametersSetAsync()
    {
        if (Client != null)
        {
            model = new ClientDto
            {
                Id = Client.Id,
                Name = Client.Name
            };
        }

        return base.OnParametersSetAsync();
    }

    private async Task Submit()
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateAction(model));
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
    }
}
