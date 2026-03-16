using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Messaging.Channels;

namespace TimeTracker.Web.Ui.Pages.Chat.Parts.Channels;

public partial class AddChannelModal
{
    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    private CreateRequest model = new()
    {
        Slug = ""
    };
    private EditForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.WorkspaceId = AuthState.Value.Workspace!.Id;
    }

    private void OnSubmit()
    {
        model.WorkspaceId = AuthState.Value.Workspace!.Id;
        Debug.Log("Form Submitted", _form.EditContext!.Validate());
        if (!_form.EditContext!.Validate())
        {
            StateHasChanged();
            return;
        }

        Dispatcher.Dispatch(new CreateChannelAction(model.Slug));  
    }
}
