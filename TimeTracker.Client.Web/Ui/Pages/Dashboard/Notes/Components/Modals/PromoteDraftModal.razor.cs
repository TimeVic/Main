using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

public partial class PromoteDraftModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public Guid DraftId { get; set; }

    [Parameter]
    public string InitialTitle { get; set; } = string.Empty;

    [Parameter]
    public bool IsWorkspaceVisibilityAllowed { get; set; }

    [Parameter]
    public IReadOnlyList<NoteTreeNodeDto> PrivateNodes { get; set; } = Array.Empty<NoteTreeNodeDto>();

    [Parameter]
    public EventCallback<PromoteNoteDraftRequest> OnSubmit { get; set; }

    private readonly PromoteNoteDraftRequest _model = new()
    {
        Visibility = NoteVisibility.Private
    };

    private EditForm _form = null!;
    private Guid? _initializedDraftId;
    private IReadOnlyList<NoteTreeNodeDto>? _workspaceNodes;
    private bool _isLoadingFolders;

    private IReadOnlyList<NoteTreeNodeDto> VisibleFolderNodes => _model.Visibility == NoteVisibility.Workspace
        ? (_workspaceNodes ?? Array.Empty<NoteTreeNodeDto>())
        : PrivateNodes;

    protected override void OnParametersSet()
    {
        if (_initializedDraftId != DraftId)
        {
            _initializedDraftId = DraftId;
            _model.NoteId = DraftId;
            _model.Title = InitialTitle;
            _model.Visibility = NoteVisibility.Private;
            _model.ParentId = null;
        }
    }

    private string GetVisibilityButtonClass(NoteVisibility visibility) => _model.Visibility == visibility
        ? "inline-flex items-center rounded-lg bg-white px-3 py-1.5 text-xs font-semibold text-slate-900 shadow-sm transition"
        : "inline-flex items-center rounded-lg px-3 py-1.5 text-xs font-medium text-slate-600 transition hover:text-slate-900";

    private async Task SetVisibilityAsync(NoteVisibility visibility)
    {
        if (_model.Visibility == visibility)
        {
            return;
        }

        _model.Visibility = visibility;
        _model.ParentId = null;

        if (visibility == NoteVisibility.Workspace && _workspaceNodes == null)
        {
            _isLoadingFolders = true;
            try
            {
                var response = await ApiService.NotesGetTreeAsync(new GetNotesTreeRequest
                {
                    Visibility = NoteVisibility.Workspace
                });
                _workspaceNodes = response?.Nodes?.ToList() ?? [];
            }
            catch
            {
                _workspaceNodes = [];
            }
            finally
            {
                _isLoadingFolders = false;
            }
        }
    }

    private void OnParentChanged(Guid? parentId)
    {
        _model.ParentId = parentId;
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
