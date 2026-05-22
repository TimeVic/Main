using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

public partial class RenameNoteNodeModal
{
    [Parameter]
    public bool IsOpened { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public NoteTreeNodeDto? Node { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    [Parameter]
    public EventCallback<RenameNoteNodeRequest> OnSubmit { get; set; }

    private RenameNoteNodeRequest _model = new();
    private EditForm _form = null!;
    private LumexModal _modal = null!;

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
    }

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
    }
}
