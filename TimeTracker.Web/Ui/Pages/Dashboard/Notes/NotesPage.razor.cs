using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Web.Ui.Pages.Dashboard.Notes.Models;
using TimeTracker.Web.Ui.Pages.Dashboard.Notes.Services;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Notes;

public partial class NotesPage
{
    [Inject]
    private ILogger<NotesPage> Logger { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "noteId")]
    public Guid? InitialNoteId { get; set; }

    private List<NoteTreeNodeDto> _flatNodes = new();
    private IReadOnlyList<NoteTreeNodeModel> _treeNodes = Array.Empty<NoteTreeNodeModel>();
    private readonly HashSet<Guid> _expandedNodeIds = new();
    private Guid? _selectedNoteId;
    private NoteDocumentDto? _currentDocument;
    private bool _loadingTree;
    private bool _loadingDocument;
    private bool _savingDocument;
    private bool _isCreatingNode;
    private bool _isRenamingNode;
    private bool _isSaveError;
    private string? _treeError;
    private string _editorTitle = string.Empty;
    private string _editorMarkdown = string.Empty;
    private NoteVisibility _editorVisibility = NoteVisibility.Workspace;
    private bool _isCreateFolderModalOpened;
    private bool _isCreateNoteModalOpened;
    private Guid? _activeParentId;
    private NoteTreeNodeDto? _nodeToRename;
    private NoteTreeNodeDto? _nodeToArchive;

    private bool IsDocumentDirty =>
        _currentDocument != null
        && (_editorTitle != _currentDocument.Title
            || _editorMarkdown != _currentDocument.MarkdownContent
            || _editorVisibility != _currentDocument.Visibility);

    private string SaveStateLabel
    {
        get
        {
            if (_savingDocument)
                return DashboardLocalizer["Saving"].Value;
            if (_isSaveError)
                return DashboardLocalizer["ErrorSaving"].Value;
            return IsDocumentDirty
                ? DashboardLocalizer["UnsavedChanges"].Value
                : DashboardLocalizer["Saved"].Value;
        }
    }

    private string SaveStateClass
    {
        get
        {
            if (_savingDocument)
                return "rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700";
            if (_isSaveError)
                return "rounded-full bg-red-50 px-3 py-1 text-xs font-semibold text-red-700";
            return IsDocumentDirty
                ? "rounded-full bg-amber-50 px-3 py-1 text-xs font-semibold text-amber-700"
                : "rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700";
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadTree();
        if (InitialNoteId.HasValue)
        {
            var node = _flatNodes.FirstOrDefault(item => item.Id == InitialNoteId.Value);
            if (node != null)
            {
                ExpandParents(node);
                await SelectDocument(node);
            }
        }
    }

    private async Task LoadTree()
    {
        _loadingTree = true;
        _treeError = null;
        try
        {
            var response = await ApiService.NotesGetTreeAsync(new GetNotesTreeRequest());
            _flatNodes = response?.Nodes?.ToList() ?? new List<NoteTreeNodeDto>();
            _treeNodes = NotesTreeBuilder.BuildNotesTree(_flatNodes, Logger);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to load notes tree");
            _treeError = DashboardLocalizer["Notes_LoadTreeError"].Value;
            ToastService.ShowError(DashboardLocalizer["Notes_LoadTreeError"].Value);
        }
        finally
        {
            _loadingTree = false;
        }
    }

    private Task ToggleExpanded(Guid noteId)
    {
        if (!_expandedNodeIds.Add(noteId))
        {
            _expandedNodeIds.Remove(noteId);
        }

        return Task.CompletedTask;
    }

    private async Task SelectDocument(NoteTreeNodeDto node)
    {
        if (node.Type != NoteNodeType.Document)
        {
            return;
        }

        _selectedNoteId = node.Id;
        _loadingDocument = true;
        _isSaveError = false;
        try
        {
            var document = await ApiService.NotesGetDocumentAsync(new GetNoteDocumentRequest
            {
                NoteId = node.Id
            });
            _currentDocument = document;
            _editorTitle = document?.Title ?? string.Empty;
            _editorMarkdown = document?.MarkdownContent ?? string.Empty;
            _editorVisibility = document?.Visibility ?? NoteVisibility.Workspace;
            ExpandParents(node);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to load note document {NoteId}", node.Id);
            ToastService.ShowError(DashboardLocalizer["Notes_LoadDocumentError"].Value);
            ClearEditor();
        }
        finally
        {
            _loadingDocument = false;
        }
    }

    private Task OnTitleChanged(string title)
    {
        _editorTitle = title;
        _isSaveError = false;
        return Task.CompletedTask;
    }

    private Task OnMarkdownChanged(string markdown)
    {
        _editorMarkdown = markdown;
        _isSaveError = false;
        return Task.CompletedTask;
    }

    private Task OnVisibilityChanged(NoteVisibility visibility)
    {
        _editorVisibility = visibility;
        _isSaveError = false;
        return Task.CompletedTask;
    }

    private async Task SaveDocument()
    {
        if (_currentDocument == null || !IsDocumentDirty)
        {
            return;
        }

        _savingDocument = true;
        _isSaveError = false;
        try
        {
            var document = await ApiService.NotesUpdateDocumentAsync(new UpdateNoteDocumentRequest
            {
                NoteId = _currentDocument.Id,
                Title = _editorTitle,
                MarkdownContent = _editorMarkdown,
                Visibility = _editorVisibility
            });
            if (document == null)
            {
                throw new InvalidOperationException("Notes update response is empty.");
            }

            _currentDocument = document;
            _editorTitle = document.Title;
            _editorMarkdown = document.MarkdownContent;
            _editorVisibility = document.Visibility;
            UpdateFlatNode(document);
            ToastService.ShowSuccess(DashboardLocalizer["SavedSuccessfully"].Value);
        }
        catch (Exception e)
        {
            _isSaveError = true;
            Logger.LogError(e, "Failed to save note document {NoteId}", _currentDocument.Id);
            ToastService.ShowError(DashboardLocalizer["Notes_SaveDocumentError"].Value);
        }
        finally
        {
            _savingDocument = false;
        }
    }

    private Task OpenCreateFolderModal(Guid? parentId)
    {
        _activeParentId = parentId;
        _isCreateFolderModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OpenCreateNoteModal(Guid? parentId)
    {
        _activeParentId = parentId;
        _isCreateNoteModalOpened = true;
        return Task.CompletedTask;
    }

    private async Task CreateFolder(CreateNoteFolderRequest request)
    {
        _isCreatingNode = true;
        try
        {
            var createdNode = await ApiService.NotesCreateFolderAsync(request);
            if (createdNode?.ParentId != null)
            {
                _expandedNodeIds.Add(createdNode.ParentId.Value);
            }
            await LoadTree();
            _isCreateFolderModalOpened = false;
            _activeParentId = null;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to create notes folder");
            ToastService.ShowError(DashboardLocalizer["Notes_CreateFolderError"].Value);
        }
        finally
        {
            _isCreatingNode = false;
        }
    }

    private async Task CreateDocument(CreateNoteDocumentRequest request)
    {
        _isCreatingNode = true;
        try
        {
            var document = await ApiService.NotesCreateDocumentAsync(request);
            if (document == null)
            {
                throw new InvalidOperationException("Notes create response is empty.");
            }

            if (document.ParentId != null)
            {
                _expandedNodeIds.Add(document.ParentId.Value);
            }
            await LoadTree();
            _isCreateNoteModalOpened = false;
            _activeParentId = null;

            var node = _flatNodes.FirstOrDefault(item => item.Id == document.Id);
            if (node != null)
            {
                await SelectDocument(node);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to create note document");
            ToastService.ShowError(DashboardLocalizer["Notes_CreateNoteError"].Value);
        }
        finally
        {
            _isCreatingNode = false;
        }
    }

    private Task OpenRenameModal(NoteTreeNodeDto node)
    {
        _nodeToRename = node;
        return Task.CompletedTask;
    }

    private async Task RenameNode(RenameNoteNodeRequest request)
    {
        _isRenamingNode = true;
        try
        {
            var updatedNode = await ApiService.NotesRenameNodeAsync(request);
            if (updatedNode != null)
            {
                ReplaceFlatNode(updatedNode);
                if (_currentDocument?.Id == updatedNode.Id)
                {
                    _currentDocument.Title = updatedNode.Title;
                    _editorTitle = updatedNode.Title;
                }
            }
            _nodeToRename = null;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to rename note node {NoteId}", request.NoteId);
            ToastService.ShowError(DashboardLocalizer["Notes_RenameNodeError"].Value);
        }
        finally
        {
            _isRenamingNode = false;
        }
    }

    private Task OpenArchiveConfirmation(NoteTreeNodeDto node)
    {
        _nodeToArchive = node;
        return Task.CompletedTask;
    }

    private async Task ArchiveNode()
    {
        if (_nodeToArchive == null)
        {
            return;
        }

        try
        {
            await ApiService.NotesArchiveNodeAsync(new ArchiveNoteNodeRequest
            {
                NoteId = _nodeToArchive.Id
            });
            _nodeToArchive = null;
            await LoadTree();
            if (_selectedNoteId.HasValue && _flatNodes.All(item => item.Id != _selectedNoteId.Value))
            {
                ClearEditor();
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to archive note node");
            ToastService.ShowError(DashboardLocalizer["Notes_ArchiveNodeError"].Value);
        }
    }

    private Task OnCreateFolderModalChanged(bool isOpened)
    {
        _isCreateFolderModalOpened = isOpened;
        if (!isOpened)
        {
            _activeParentId = null;
        }

        return Task.CompletedTask;
    }

    private Task OnCreateNoteModalChanged(bool isOpened)
    {
        _isCreateNoteModalOpened = isOpened;
        if (!isOpened)
        {
            _activeParentId = null;
        }

        return Task.CompletedTask;
    }

    private Task OnRenameModalChanged(bool isOpened)
    {
        if (!isOpened)
        {
            _nodeToRename = null;
        }

        return Task.CompletedTask;
    }

    private void UpdateFlatNode(NoteDocumentDto document)
    {
        var node = _flatNodes.FirstOrDefault(item => item.Id == document.Id);
        if (node == null)
        {
            return;
        }

        node.Title = document.Title;
        node.Visibility = document.Visibility;
        node.UpdatedAt = document.UpdatedAt;
        _treeNodes = NotesTreeBuilder.BuildNotesTree(_flatNodes, Logger);
    }

    private void ReplaceFlatNode(NoteTreeNodeDto updatedNode)
    {
        var index = _flatNodes.FindIndex(item => item.Id == updatedNode.Id);
        if (index >= 0)
        {
            _flatNodes[index] = updatedNode;
        }
        else
        {
            _flatNodes.Add(updatedNode);
        }

        _treeNodes = NotesTreeBuilder.BuildNotesTree(_flatNodes, Logger);
    }

    private void ExpandParents(NoteTreeNodeDto node)
    {
        var parentId = node.ParentId;
        while (parentId.HasValue)
        {
            _expandedNodeIds.Add(parentId.Value);
            parentId = _flatNodes.FirstOrDefault(item => item.Id == parentId.Value)?.ParentId;
        }
    }

    private void ClearEditor()
    {
        _selectedNoteId = null;
        _currentDocument = null;
        _editorTitle = string.Empty;
        _editorMarkdown = string.Empty;
        _editorVisibility = NoteVisibility.Workspace;
    }
}
