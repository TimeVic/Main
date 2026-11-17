using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Models;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components;

public partial class NotesTreeBlock
{
    [Parameter]
    public NoteVisibility ActiveMode { get; set; } = NoteVisibility.Workspace;

    [Parameter]
    public IReadOnlyList<NoteTreeNodeModel> TreeNodes { get; set; } = Array.Empty<NoteTreeNodeModel>();

    [Parameter]
    public IReadOnlyList<NoteTreeNodeDto> Drafts { get; set; } = Array.Empty<NoteTreeNodeDto>();

    [Parameter]
    public HashSet<Guid> ExpandedNodeIds { get; set; } = new();

    [Parameter]
    public Guid? SelectedNoteId { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool IsCreatingDraft { get; set; }

    [Parameter]
    public bool IsDeletingDraft { get; set; }

    [Parameter]
    public bool IsEmbedded { get; set; }

    [Parameter]
    public bool IsEditingAllowed { get; set; }

    [Parameter]
    public EventCallback<Guid> ToggleExpandedRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> SelectNodeRequested { get; set; }

    [Parameter]
    public EventCallback<Guid?> CreateFolderRequested { get; set; }

    [Parameter]
    public EventCallback<Guid?> CreateNoteRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> RenameRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> MoveRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> ArchiveRequested { get; set; }

    [Parameter]
    public EventCallback CreateDraftRequested { get; set; }

    [Parameter]
    public EventCallback<Guid> DeleteDraftRequested { get; set; }

    private string ContainerClass => IsEmbedded
        ? "flex min-h-[720px] w-full flex-col bg-white"
        : "min-h-[720px] rounded-2xl border border-slate-200 bg-white shadow-sm";
}
