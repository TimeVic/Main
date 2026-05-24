using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

public partial class CreateFolderModal
{
    [Parameter]
    public bool IsOpened { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public Guid? ParentId { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    [Parameter]
    public EventCallback<CreateNoteFolderRequest> OnSubmit { get; set; }

    private readonly CreateNoteFolderRequest _model = new()
    {
        Visibility = NoteVisibility.Workspace
    };

    private EditForm _form = null!;
    private LumexModal _modal = null!;

    protected override void OnParametersSet()
    {
        _model.ParentId = ParentId;
        if (!IsOpened)
        {
            ResetModel();
        }
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

    private void OnVisibilityChanged(NoteVisibility? visibility)
    {
        _model.Visibility = visibility ?? NoteVisibility.Workspace;
    }

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
    }

    private void ResetModel()
    {
        _model.ParentId = ParentId;
        _model.Title = string.Empty;
        _model.Visibility = NoteVisibility.Workspace;
        _model.SortOrder = null;
    }
}
