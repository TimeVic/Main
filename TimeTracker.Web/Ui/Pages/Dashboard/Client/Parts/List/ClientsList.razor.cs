using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.Client;
using LoadListAction = TimeTracker.Web.Store.Client.LoadListAction;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Client.Parts.List;

public partial class ClientsList
{
    [Inject] 
    private IState<ClientState> _state { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadListAction(true));
    }
    
    private async Task OnAdd()
    {
        await ModalDialogService.ShowAddClientModal();
    }
}
