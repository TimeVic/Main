using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Messaging.Channels;

namespace TimeTracker.Web.Ui.Pages.Chat.Parts.Channels;

public partial class AddChannelModal
{
    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    private LumexModal modal;
    
    private CreateRequest model = new()
    {
        Slug = ""
    };
    private EditForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void OnSubmit()
    {
        if (!_form.EditContext!.Validate())
        {
            StateHasChanged();
            return;
        }

        Dispatcher.Dispatch(new CreateChannelAction(model.Slug));
        modal.CloseAsync();
    }
}
