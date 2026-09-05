using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

public partial class RenameNoteNodeModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public NoteTreeNodeDto? Node { get; set; }

    [Parameter]
    public EventCallback<RenameNoteNodeRequest> OnSubmit { get; set; }

    private RenameNoteNodeRequest _model = new();
    private EditForm _form = null!;

    protected override void OnParametersSet()
    {
        if (Node == null)
        {
            return;
        }

        _model = new RenameNoteNodeRequest
        {
            NoteId = Node.Id,
            Title = Node.Title
        };
    }

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

        await OnSubmit.InvokeAsync(_model);
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok(_model));
        }
    }
}
