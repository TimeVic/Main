using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Client;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class ClientsBlock
{
    [Inject] 
    private IState<ClientState> _state { get; set; }

    private Task OnSave(ClientDto context)
    {
        return Task.CompletedTask;
    }
}
