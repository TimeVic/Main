using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Web.Store.WorkspaceMembers;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class AddMemberForm
{
    [Inject]
    private IState<WorkspaceMembersState> _state { get; set; } = default!;

    private InviteModel _model = new();
    private EditForm _form = default!;

    private Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return Task.CompletedTask;
        }

        Dispatcher.Dispatch(new AddNewMemberAction(_model.Email));
        _model = new InviteModel();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private class InviteModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
