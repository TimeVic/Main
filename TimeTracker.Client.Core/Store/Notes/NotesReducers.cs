using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Client.Core.Store.Notes;

public class NotesReducers
{
    [ReducerMethod]
    public static NotesState SetNotesTreeReducer(NotesState state, SetNotesTreeAction action)
    {
        return state with
        {
            FlatNodes = action.Nodes,
            IsTreeLoaded = true
        };
    }

    [ReducerMethod]
    public static NotesState SetNotesTreeLoadingReducer(NotesState state, SetNotesTreeLoadingAction action)
    {
        return state with
        {
            IsTreeLoading = action.IsLoading
        };
    }

    [ReducerMethod]
    public static NotesState SetNotesTreeErrorReducer(NotesState state, SetNotesTreeErrorAction action)
    {
        return state with
        {
            TreeErrorLocalizationKey = action.LocalizationKey
        };
    }

    [ReducerMethod]
    public static NotesState ToggleNoteExpandedReducer(NotesState state, ToggleNoteExpandedAction action)
    {
        var expandedNodeIds = state.ExpandedNodeIds.ToHashSet();
        if (!expandedNodeIds.Add(action.NoteId))
        {
            expandedNodeIds.Remove(action.NoteId);
        }

        return state with
        {
            ExpandedNodeIds = expandedNodeIds
        };
    }

    [ReducerMethod]
    public static NotesState ExpandNoteParentsReducer(NotesState state, ExpandNoteParentsAction action)
    {
        var expandedNodeIds = state.ExpandedNodeIds.ToHashSet();
        var parentId = state.FlatNodes.FirstOrDefault(item => item.Id == action.NoteId)?.ParentId;
        while (parentId.HasValue)
        {
            expandedNodeIds.Add(parentId.Value);
            parentId = state.FlatNodes.FirstOrDefault(item => item.Id == parentId.Value)?.ParentId;
        }

        return state with
        {
            ExpandedNodeIds = expandedNodeIds
        };
    }

    [ReducerMethod]
    public static NotesState SetNoteDocumentReducer(NotesState state, SetNoteDocumentAction action)
    {
        return state with
        {
            SelectedNoteId = action.Document.Id,
            CurrentDocument = action.Document,
            CurrentContent = action.Content,
            EditorTitle = action.Document.Title,
            EditorMarkdown = action.Content.MarkdownContent,
            EditorVisibility = action.Document.Visibility,
            IsEditingDocument = action.IsEditing,
            IsSaveError = false
        };
    }

    [ReducerMethod]
    public static NotesState SetNoteDocumentAttachmentsReducer(
        NotesState state,
        SetNoteDocumentAttachmentsAction action
    )
    {
        if (state.CurrentDocument == null)
        {
            return state;
        }

        var document = state.CurrentDocument;
        return state with
        {
            CurrentDocument = new NoteDocumentDto
            {
                Id = document.Id,
                ParentId = document.ParentId,
                Title = document.Title,
                LastContentId = document.LastContentId,
                Visibility = document.Visibility,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                Links = document.Links,
                Attachments = action.Attachments
            }
        };
    }

    [ReducerMethod]
    public static NotesState SetNoteDocumentLoadingReducer(NotesState state, SetNoteDocumentLoadingAction action)
    {
        return state with
        {
            IsDocumentLoading = action.IsLoading
        };
    }

    [ReducerMethod]
    public static NotesState SetNoteEditorTitleReducer(NotesState state, SetNoteEditorTitleAction action)
    {
        return state with
        {
            EditorTitle = action.Title,
            IsSaveError = false
        };
    }

    [ReducerMethod]
    public static NotesState SetNoteEditorMarkdownReducer(NotesState state, SetNoteEditorMarkdownAction action)
    {
        return state with
        {
            EditorMarkdown = action.Markdown,
            IsSaveError = false
        };
    }

    [ReducerMethod]
    public static NotesState SetNoteEditorVisibilityReducer(NotesState state, SetNoteEditorVisibilityAction action)
    {
        return state with
        {
            EditorVisibility = action.Visibility,
            IsSaveError = false
        };
    }

    [ReducerMethod]
    public static NotesState StartNoteDocumentEditingReducer(NotesState state, StartNoteDocumentEditingAction action)
    {
        return state.CurrentDocument == null
            ? state
            : state with
            {
                IsEditingDocument = true
            };
    }

    [ReducerMethod]
    public static NotesState CancelNoteDocumentEditingReducer(NotesState state, CancelNoteDocumentEditingAction action)
    {
        return state.CurrentDocument == null
            ? state
            : state with
            {
                EditorTitle = state.CurrentDocument.Title,
                EditorMarkdown = state.CurrentContent?.MarkdownContent ?? string.Empty,
                EditorVisibility = state.CurrentDocument.Visibility,
                IsEditingDocument = false,
                IsSaveError = false
            };
    }

    [ReducerMethod]
    public static NotesState SetNoteDocumentSavingReducer(NotesState state, SetNoteDocumentSavingAction action)
    {
        return state with
        {
            IsDocumentSaving = action.IsSaving
        };
    }

    [ReducerMethod]
    public static NotesState SetNoteSaveErrorReducer(NotesState state, SetNoteSaveErrorAction action)
    {
        return state with
        {
            IsSaveError = action.IsSaveError
        };
    }

    [ReducerMethod]
    public static NotesState ClearNoteEditorReducer(NotesState state, ClearNoteEditorAction action)
    {
        return state with
        {
            SelectedNoteId = null,
            CurrentDocument = null,
            CurrentContent = null,
            EditorTitle = string.Empty,
            EditorMarkdown = string.Empty,
            EditorVisibility = NoteVisibility.Workspace,
            IsEditingDocument = false,
            IsSaveError = false
        };
    }

    [ReducerMethod]
    public static NotesState OpenCreateNoteFolderModalReducer(NotesState state, OpenCreateNoteFolderModalAction action)
    {
        return state with
        {
            ActiveParentId = action.ParentId,
            IsCreateFolderModalOpened = true
        };
    }

    [ReducerMethod]
    public static NotesState SetCreateNoteFolderModalOpenedReducer(NotesState state, SetCreateNoteFolderModalOpenedAction action)
    {
        return state with
        {
            IsCreateFolderModalOpened = action.IsOpened,
            ActiveParentId = action.IsOpened ? state.ActiveParentId : null
        };
    }

    [ReducerMethod]
    public static NotesState OpenCreateNoteDocumentModalReducer(NotesState state, OpenCreateNoteDocumentModalAction action)
    {
        return state with
        {
            ActiveParentId = action.ParentId,
            IsCreateNoteModalOpened = true
        };
    }

    [ReducerMethod]
    public static NotesState SetCreateNoteDocumentModalOpenedReducer(NotesState state, SetCreateNoteDocumentModalOpenedAction action)
    {
        return state with
        {
            IsCreateNoteModalOpened = action.IsOpened,
            ActiveParentId = action.IsOpened ? state.ActiveParentId : null
        };
    }

    [ReducerMethod]
    public static NotesState SetNotesNodeCreatingReducer(NotesState state, SetNotesNodeCreatingAction action)
    {
        return state with
        {
            IsCreatingNode = action.IsCreating
        };
    }

    [ReducerMethod]
    public static NotesState OpenRenameNoteNodeModalReducer(NotesState state, OpenRenameNoteNodeModalAction action)
    {
        return state with
        {
            NodeToRename = action.Node
        };
    }

    [ReducerMethod]
    public static NotesState SetRenameNoteNodeModalOpenedReducer(NotesState state, SetRenameNoteNodeModalOpenedAction action)
    {
        return state with
        {
            NodeToRename = action.IsOpened ? state.NodeToRename : null
        };
    }

    [ReducerMethod]
    public static NotesState SetNotesNodeRenamingReducer(NotesState state, SetNotesNodeRenamingAction action)
    {
        return state with
        {
            IsRenamingNode = action.IsRenaming
        };
    }

    [ReducerMethod]
    public static NotesState OpenMoveNoteNodeModalReducer(NotesState state, OpenMoveNoteNodeModalAction action)
    {
        return state with
        {
            NodeToMove = action.Node
        };
    }

    [ReducerMethod]
    public static NotesState SetMoveNoteNodeModalOpenedReducer(NotesState state, SetMoveNoteNodeModalOpenedAction action)
    {
        return state with
        {
            NodeToMove = action.IsOpened ? state.NodeToMove : null
        };
    }

    [ReducerMethod]
    public static NotesState SetNotesNodeMovingReducer(NotesState state, SetNotesNodeMovingAction action)
    {
        return state with
        {
            IsMovingNode = action.IsMoving
        };
    }

    [ReducerMethod]
    public static NotesState OpenArchiveNoteNodeConfirmationReducer(NotesState state, OpenArchiveNoteNodeConfirmationAction action)
    {
        return state with
        {
            NodeToArchive = action.Node
        };
    }

    [ReducerMethod]
    public static NotesState SetArchiveNoteNodeConfirmationOpenedReducer(NotesState state, SetArchiveNoteNodeConfirmationOpenedAction action)
    {
        return state with
        {
            NodeToArchive = action.IsOpened ? state.NodeToArchive : null
        };
    }

    [ReducerMethod]
    public static NotesState ReplaceNoteTreeNodeReducer(NotesState state, ReplaceNoteTreeNodeAction action)
    {
        var flatNodes = ReplaceNode(state.FlatNodes, action.Node);
        var currentDocument = state.CurrentDocument;
        var editorTitle = state.EditorTitle;
        if (currentDocument?.Id == action.Node.Id)
        {
            currentDocument = new NoteDocumentDto
            {
                Id = currentDocument.Id,
                ParentId = action.Node.ParentId,
                Title = action.Node.Title,
                LastContentId = currentDocument.LastContentId,
                Visibility = currentDocument.Visibility,
                CreatedAt = currentDocument.CreatedAt,
                UpdatedAt = currentDocument.UpdatedAt,
                Links = currentDocument.Links
            };
            editorTitle = action.Node.Title;
        }

        return state with
        {
            FlatNodes = flatNodes,
            CurrentDocument = currentDocument,
            EditorTitle = editorTitle,
            NodeToRename = null
        };
    }

    [ReducerMethod]
    public static NotesState UpdateNoteTreeNodeFromDocumentReducer(NotesState state, UpdateNoteTreeNodeFromDocumentAction action)
    {
        var existingNode = state.FlatNodes.FirstOrDefault(item => item.Id == action.Document.Id);
        if (existingNode == null)
        {
            return state;
        }

        var updatedNode = new NoteTreeNodeDto
        {
            Id = existingNode.Id,
            ParentId = existingNode.ParentId,
            Type = existingNode.Type,
            Title = action.Document.Title,
            LastContentId = action.Document.LastContentId,
            Visibility = action.Document.Visibility,
            SortOrder = existingNode.SortOrder,
            UpdatedAt = action.Document.UpdatedAt
        };

        return state with
        {
            FlatNodes = ReplaceNode(state.FlatNodes, updatedNode)
        };
    }

    private static IReadOnlyList<NoteTreeNodeDto> ReplaceNode(
        IReadOnlyList<NoteTreeNodeDto> flatNodes,
        NoteTreeNodeDto updatedNode
    )
    {
        var nodes = flatNodes.ToList();
        var index = nodes.FindIndex(item => item.Id == updatedNode.Id);
        if (index >= 0)
        {
            nodes[index] = updatedNode;
        }
        else
        {
            nodes.Add(updatedNode);
        }

        return nodes;
    }
}
