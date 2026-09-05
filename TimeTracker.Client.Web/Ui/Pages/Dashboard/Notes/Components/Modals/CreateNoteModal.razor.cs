using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

public partial class CreateNoteModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public Guid? ParentId { get; set; }

    [Parameter]
    public NoteVisibility Visibility { get; set; } = NoteVisibility.Workspace;

    [Parameter]
    public EventCallback<CreateNoteDocumentRequest> OnSubmit { get; set; }

    private readonly CreateNoteDocumentRequest _model = new()
    {
        Visibility = NoteVisibility.Workspace
    };

    private EditForm _form = null!;

    protected override void OnParametersSet()
    {
        _model.ParentId = ParentId;
        _model.Visibility = Visibility;
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
