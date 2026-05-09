using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Web.Store.Client;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Clients;

public partial class AddClientModal
{
    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }

    [Inject]
    public IState<ClientState> _state { get; set; }

    private AddRequest model = new();
    private EditForm _form;
    private LumexModal modal;

    private Task SubmitForm(EditContext editContext)
    {
        return Submit();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new AddAction(model));
        await OnCloseModal();
        StateHasChanged();
    }

    private async Task OnCloseModal()
    {
        model = new AddRequest();
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
