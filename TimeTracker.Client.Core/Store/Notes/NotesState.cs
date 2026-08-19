using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Client.Core.Store.Notes;

[FeatureState]
public record NotesState
{
    public IReadOnlyList<NoteTreeNodeDto> FlatNodes { get; init; } = [];

    public NoteVisibility ActiveMode { get; init; } = NoteVisibility.Workspace;

    public HashSet<Guid> ExpandedNodeIds { get; init; } = [];

    public Guid? SelectedNoteId { get; init; }

    public NoteDocumentDto? CurrentDocument { get; init; }

    public NoteContentDto? CurrentContent { get; init; }

    public string EditorTitle { get; init; } = string.Empty;

    public string EditorMarkdown { get; init; } = string.Empty;

    public NoteVisibility EditorVisibility { get; init; } = NoteVisibility.Workspace;

    public bool IsTreeLoaded { get; init; }

    public bool IsTreeLoading { get; init; }

    public bool IsDocumentLoading { get; init; }

    public bool IsDocumentSaving { get; init; }

    public bool IsEditingDocument { get; init; }

    public bool IsCreatingNode { get; init; }

    public bool IsRenamingNode { get; init; }

    public bool IsMovingNode { get; init; }

    public bool IsSaveError { get; init; }

    public string? TreeErrorLocalizationKey { get; init; }

    public bool IsCreateFolderModalOpened { get; init; }

    public bool IsCreateNoteModalOpened { get; init; }

    public Guid? ActiveParentId { get; init; }

    public NoteTreeNodeDto? NodeToRename { get; init; }

    public NoteTreeNodeDto? NodeToMove { get; init; }

    public NoteTreeNodeDto? NodeToArchive { get; init; }

    public bool IsDocumentDirty =>
        CurrentDocument != null
        && CurrentContent != null
        && (EditorTitle != CurrentDocument.Title
            || EditorMarkdown != CurrentContent.MarkdownContent
            || EditorVisibility != CurrentDocument.Visibility);
}
