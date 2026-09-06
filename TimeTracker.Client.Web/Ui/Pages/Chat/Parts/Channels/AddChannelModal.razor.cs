using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Messaging.Channels;

namespace TimeTracker.Client.Web.Ui.Pages.Chat.Parts.Channels;

public partial class AddChannelModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Inject]
    protected IState<AuthState> AuthState { get; set; } = default!;
    
    private CreateRequest model = new()
    {
        Slug = ""
    };
    private EditForm _form = default!;

    private async Task OnSubmit()
    {
        if (!_form.EditContext!.Validate())
        {
            StateHasChanged();
            return;
        }

        Dispatcher.Dispatch(new CreateChannelAction(model.Slug));
        model = new CreateRequest { Slug = "" };
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
    }
}
