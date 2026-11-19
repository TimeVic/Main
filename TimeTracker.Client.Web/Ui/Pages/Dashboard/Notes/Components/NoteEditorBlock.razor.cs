using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Ui.Shared.Components.Form;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components;

public partial class NoteEditorBlock
{
    [Parameter]
    public NoteDocumentDto? Document { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string MarkdownContent { get; set; } = string.Empty;

    [Parameter]
    public NoteVisibility Visibility { get; set; } = NoteVisibility.Workspace;

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public bool IsDirty { get; set; }

    [Parameter]
    public bool IsEditing { get; set; }

    [Parameter]
    public bool IsEditingAllowed { get; set; }

    [Parameter]
    public bool IsEmbedded { get; set; }

    [Parameter]
    public string SaveStateLabel { get; set; } = string.Empty;

    [Parameter]
    public string SaveStateClass { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> TitleChanged { get; set; }

    [Parameter]
    public EventCallback<string> MarkdownContentChanged { get; set; }

    [Parameter]
    public EventCallback OnEdit { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public EventCallback OnSave { get; set; }

    [Parameter]
    public EventCallback<ICollection<StoredFileDto>> AttachmentsChanged { get; set; }

    [Parameter]
    public bool IsDeletingDraft { get; set; }

    [Parameter]
    public bool IsPromotingDraft { get; set; }

    [Parameter]
    public bool ShouldFocusDraftEditor { get; set; }

    [Parameter]
    public EventCallback OnSaveDraft { get; set; }

    [Parameter]
    public EventCallback<Guid> OnDeleteDraft { get; set; }

    [Parameter]
    public EventCallback OnPromoteDraft { get; set; }

    [Parameter]
    public EventCallback OnDraftEditorFocused { get; set; }

    [Inject]
    private UiHelperService UiHelperService { get; set; } = null!;

    private MarkdownEdit? _markdownEdit;
    private bool _hasHandledDraftFocus;
    private Guid? _focusedDraftNoteId;

    private bool IsEffectiveEditing => Document != null && (Document.IsDraft || IsEditing);

    private string ContainerClass => IsEmbedded
        ? "flex min-h-[720px] w-full flex-col bg-white"
        : "flex min-h-[720px] flex-col rounded-2xl border border-slate-200 bg-white shadow-sm";

    protected override void OnParametersSet()
    {
        if (!ShouldFocusDraftEditor || Document?.Id != _focusedDraftNoteId)
        {
            _hasHandledDraftFocus = false;
            _focusedDraftNoteId = null;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!ShouldFocusDraftEditor || _hasHandledDraftFocus || Document?.IsDraft != true || _markdownEdit == null)
        {
            return;
        }

        // Guard before dispatching focus clear: OnDraftEditorFocused dispatches inside Fluxor's active
        // CreateDraft dispatch pass, which defers the state update and otherwise re-enters OnAfterRenderAsync.
        _hasHandledDraftFocus = true;
        _focusedDraftNoteId = Document.Id;

        try
        {
            await _markdownEdit.FocusAsync();
        }
        finally
        {
            await OnDraftEditorFocused.InvokeAsync();
        }
    }

    private async Task OnTitleInput(ChangeEventArgs args)
    {
        await TitleChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);
    }

    private async Task OnTitleBlur()
    {
        if (Document?.IsDraft == true)
        {
            await OnSaveDraft.InvokeAsync();
        }
    }

    private async Task OnMarkdownChanged(string value)
    {
        await MarkdownContentChanged.InvokeAsync(value);
    }

    private async Task OnMarkdownBlur()
    {
        if (Document?.IsDraft == true)
        {
            await OnSaveDraft.InvokeAsync();
        }
    }

    private async Task OnCopyAllClick()
    {
        var isCopied = await UiHelperService.CopyToClipboard(MarkdownContent);
        if (isCopied)
        {
            ToastService.ShowSuccess(DashboardLocalizer["Notes_CopiedToClipboard"].Value);
            return;
        }

        ToastService.ShowError(DashboardLocalizer["Notes_CopyToClipboardError"].Value);
    }

    private async Task OnPromoteDraftClick()
    {
        await OnPromoteDraft.InvokeAsync();
    }

    private async Task OnDeleteDraftClick()
    {
        if (Document == null || !Document.IsDraft)
        {
            return;
        }

        await OnDeleteDraft.InvokeAsync(Document.Id);
    }

    private async Task OnEditClick()
    {
        await OnEdit.InvokeAsync();
    }

    private async Task OnCancelClick()
    {
        await OnCancel.InvokeAsync();
    }
}
