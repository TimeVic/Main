using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Client.Core.Store.Notes;

public record struct LoadNotesTreeAction(bool IsReload = false, Guid? InitialNoteId = null);

public record struct SetNotesActiveModeAction(NoteVisibility Mode);

public record struct SetNotesTreeAction(IReadOnlyList<NoteTreeNodeDto> Nodes);

public record struct SetNotesTreeLoadingAction(bool IsLoading);

public record struct SetNotesTreeErrorAction(string? LocalizationKey);

public record struct ToggleNoteExpandedAction(Guid NoteId);

public record struct ExpandNoteParentsAction(Guid NoteId);

public record struct SelectNoteDocumentAction(Guid NoteId);

public record struct SetNoteDocumentAction(NoteDocumentDto Document, NoteContentDto Content, bool IsEditing);

public record struct SetNoteDocumentAttachmentsAction(ICollection<StoredFileDto> Attachments);

public record struct SetNoteDocumentLoadingAction(bool IsLoading);

public record struct SetNoteEditorTitleAction(string Title);

public record struct SetNoteEditorMarkdownAction(string Markdown);

public record struct SetNoteEditorVisibilityAction(NoteVisibility Visibility);

public record struct StartNoteDocumentEditingAction;

public record struct CancelNoteDocumentEditingAction;

public record struct SaveNoteDocumentAction;

public record struct SetNoteDocumentSavingAction(bool IsSaving);

public record struct SetNoteSaveErrorAction(bool IsSaveError);

public record struct ClearNoteEditorAction;

public record struct OpenCreateNoteFolderModalAction(Guid? ParentId);

public record struct SetCreateNoteFolderModalOpenedAction(bool IsOpened);

public record struct OpenCreateNoteDocumentModalAction(Guid? ParentId);

public record struct SetCreateNoteDocumentModalOpenedAction(bool IsOpened);

public record struct CreateNoteFolderAction(CreateNoteFolderRequest Request);

public record struct CreateNoteDocumentAction(CreateNoteDocumentRequest Request);

public record struct SetNotesNodeCreatingAction(bool IsCreating);

public record struct OpenRenameNoteNodeModalAction(NoteTreeNodeDto Node);

public record struct SetRenameNoteNodeModalOpenedAction(bool IsOpened);

public record struct RenameNoteNodeAction(RenameNoteNodeRequest Request);

public record struct SetNotesNodeRenamingAction(bool IsRenaming);

public record struct OpenMoveNoteNodeModalAction(NoteTreeNodeDto Node);

public record struct SetMoveNoteNodeModalOpenedAction(bool IsOpened);

public record struct MoveNoteNodeAction(MoveNoteNodeRequest Request);

public record struct SetNotesNodeMovingAction(bool IsMoving);

public record struct OpenArchiveNoteNodeConfirmationAction(NoteTreeNodeDto Node);

public record struct SetArchiveNoteNodeConfirmationOpenedAction(bool IsOpened);

public record struct ArchiveNoteNodeAction;

public record struct ReplaceNoteTreeNodeAction(NoteTreeNodeDto Node);

public record struct UpdateNoteTreeNodeFromDocumentAction(NoteDocumentDto Document);
