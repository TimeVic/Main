using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class AddMemberModal
{
    private sealed class InviteModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    [Inject]
    private IState<WorkspaceMembersState> _state { get; set; } = default!;

    private InviteModel _model = new();
    private EditForm _form = default!;

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new AddNewMemberAction(_model.Email));
        await OnCloseModal();
    }

    private async Task OnCloseModal()
    {
        _model = new InviteModel();
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
