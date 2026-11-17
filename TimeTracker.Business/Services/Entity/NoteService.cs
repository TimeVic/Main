using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dto.Notes;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Business.Services.Entity;

/// <summary>
/// Implements note node, draft, link, and attachment manipulation and domain validations.
/// </summary>
public class NoteService : INoteService
{
    private const int TitleMaxLength = 200;
    private const int MarkdownMaxLength = 5_000_000;

    private readonly ISecurityManager _securityManager;
    private readonly INoteDao _noteDao;
    private readonly IClientDao _clientDao;
    private readonly IProjectDao _projectDao;
    private readonly ITaskDao _taskDao;
    private readonly IFileStorage _fileStorage;

    public NoteService(
        ISecurityManager securityManager,
        INoteDao noteDao,
        IClientDao clientDao,
        IProjectDao projectDao,
        ITaskDao taskDao,
        IFileStorage fileStorage
    )
    {
        _securityManager = securityManager;
        _noteDao = noteDao;
        _clientDao = clientDao;
        _projectDao = projectDao;
        _taskDao = taskDao;
        _fileStorage = fileStorage;
    }

    public async Task<NoteTreeResultDto> GetTreeAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        bool isIncludeArchived,
        NoteVisibility visibility
    )
    {
        var nodes = await _noteDao.GetTreeAsync(workspace, isIncludeArchived, visibility);
        var availableNodes = await GetAvailableNotesAsync(user, nodes, AccessLevel.Read);
        var availableNodeIds = availableNodes.Select(item => item.Id).ToHashSet();

        foreach (var availableNode in availableNodes)
        {
            var parent = availableNode.Parent;
            while (parent != null && availableNodeIds.Add(parent.Id))
            {
                parent = parent.Parent;
            }
        }

        var visibleNodes = nodes.Where(item => availableNodeIds.Contains(item.Id)).ToList();
        var treeNodes = visibleNodes.Where(item => !item.IsDraft).ToList();
        var drafts = visibility == NoteVisibility.Private
            ? visibleNodes
                .Where(item => item.IsDraft && !item.IsArchived)
                .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                .ToList()
            : [];

        return new NoteTreeResultDto
        {
            Nodes = treeNodes,
            Drafts = drafts
        };
    }

    public async Task<NoteNodeEntity> GetDocumentAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId)
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Read);
        EnsureDocument(note);
        return note;
    }

    public async Task<NoteContentEntity> GetContentAsync(WorkspaceEntity workspace, UserEntity user, Guid contentId)
    {
        var content = await _noteDao.GetContentByIdAsync(workspace, contentId);
        RecordNotFoundException.ThrowIfNull(content);
        EnsureDocument(content.NoteNode);
        await _securityManager.CheckAccess(AccessLevel.Read, user, content.NoteNode);
        return content;
    }

    public async Task<ICollection<NoteNodeHistoryEntity>> GetHistoryAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId
    )
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Read);
        EnsureDocument(note);
        return await _noteDao.GetHistoryAsync(note);
    }

    public async Task<NoteNodeEntity> CreateFolderAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string title,
        NoteVisibility visibility,
        Guid? parentId,
        int? sortOrder
    )
    {
        await EnsureCanUseVisibilityAsync(workspace, user, visibility);

        var parent = await GetValidParentAsync(workspace, user, parentId);
        EnsureMatchingParentVisibility(parent, visibility);

        return await _noteDao.CreateNodeAsync(
            workspace,
            parent,
            user,
            NoteNodeType.Folder,
            NormalizeTitle(title),
            null,
            visibility,
            await ResolveSortOrderAsync(workspace, parent, sortOrder)
        );
    }

    public async Task<NoteNodeEntity> CreateDocumentAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string title,
        NoteVisibility visibility,
        Guid? parentId,
        int? sortOrder,
        ICollection<NoteLinkTargetDto>? links = null
    )
    {
        await EnsureCanUseVisibilityAsync(workspace, user, visibility);

        var parent = await GetValidParentAsync(workspace, user, parentId);
        EnsureMatchingParentVisibility(parent, visibility);

        var normalizedLinks = links ?? Array.Empty<NoteLinkTargetDto>();
        EnsureNoDuplicateLinks(normalizedLinks);

        foreach (var link in normalizedLinks)
        {
            await EnsureLinkedEntityExistsAsync(workspace, link.EntityType, link.EntityId);
        }

        var note = await _noteDao.CreateNodeAsync(
            workspace,
            parent,
            user,
            NoteNodeType.Document,
            NormalizeTitle(title),
            string.Empty,
            visibility,
            await ResolveSortOrderAsync(workspace, parent, sortOrder)
        );

        foreach (var link in normalizedLinks)
        {
            await _noteDao.CreateLinkAsync(workspace, note, user, link.EntityType, link.EntityId);
        }

        return note;
    }

    public async Task<NoteNodeEntity> UpdateDocumentAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string title
    )
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureDocument(note);
        EnsureNotDraft(note);

        note.Title = NormalizeTitle(title);
        SetUpdatedBy(note, user);
        await _noteDao.SaveNodeAsync(note);

        return note;
    }

    public async Task<NoteContentEntity> UpdateContentAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string? markdownContent
    )
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureDocument(note);
        EnsureNotDraft(note);

        var now = DateTime.UtcNow;
        var content = await _noteDao.CreateContentAsync(note, NormalizeMarkdown(markdownContent), now);
        SetUpdatedBy(note, user, now);
        await _noteDao.SaveNodeAsync(note);

        return content;
    }

    public async Task<NoteNodeEntity> RenameNodeAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string title
    )
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureNotDraft(note);

        note.Title = NormalizeTitle(title);
        SetUpdatedBy(note, user);
        await _noteDao.SaveNodeAsync(note);

        return note;
    }

    public async Task<NoteNodeEntity> MoveNodeAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        Guid? parentId,
        int? sortOrder
    )
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureNotDraft(note);
        await EnsureCanUseVisibilityAsync(workspace, user, note.Visibility);

        var parent = await GetValidParentAsync(workspace, user, parentId);
        EnsureMatchingParentVisibility(parent, note.Visibility);

        if (note.Type == NoteNodeType.Folder && parent != null)
        {
            await EnsureFolderIsNotMovedIntoItselfOrDescendantAsync(workspace, note, parent);
        }

        note.Parent = parent;
        note.SortOrder = await ResolveSortOrderAsync(workspace, parent, sortOrder);
        SetUpdatedBy(note, user);
        await _noteDao.SaveNodeAsync(note);

        return note;
    }

    public async Task<NoteNodeEntity> ArchiveNodeAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId)
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureNotDraft(note);
        await EnsureCanUseVisibilityAsync(workspace, user, note.Visibility);

        var allNodes = await _noteDao.GetWorkspaceNodesAsync(workspace);
        var now = DateTime.UtcNow;

        // Folder archive is recursive so archived folders cannot leave active orphaned children in the tree.
        foreach (var node in GetNodeWithDescendants(note, allNodes))
        {
            await _securityManager.CheckAccess(AccessLevel.Write, user, node);
            node.ArchivedAt ??= now;
            SetUpdatedBy(node, user, now);
            await _noteDao.SaveNodeAsync(node);
        }

        return note;
    }

    public async Task<NoteNodeEntity> CreateDraftAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string? title,
        string? markdownContent
    )
    {
        var normalizedMarkdown = NormalizeMarkdown(markdownContent);
        var resolvedTitle = string.IsNullOrWhiteSpace(title)
            ? await GenerateDraftTitleAsync(user, normalizedMarkdown)
            : NormalizeTitle(title);

        // Draft notes are always Private and never nested under folders.
        return await _noteDao.CreateNodeAsync(
            workspace,
            parent: null,
            user,
            NoteNodeType.Document,
            resolvedTitle,
            normalizedMarkdown,
            NoteVisibility.Private,
            sortOrder: 0,
            isDraft: true
        );
    }

    public async Task<NoteNodeEntity> UpdateDraftAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string? title,
        string? markdownContent
    )
    {
        var draft = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureDraft(draft);

        var now = DateTime.UtcNow;
        var currentMarkdown = draft.LastContent?.MarkdownContent ?? string.Empty;
        var nextMarkdown = markdownContent != null
            ? NormalizeMarkdown(markdownContent)
            : currentMarkdown;

        if (title != null)
        {
            draft.Title = string.IsNullOrWhiteSpace(title)
                ? await GenerateDraftTitleAsync(user, nextMarkdown)
                : NormalizeTitle(title);
        }

        if (markdownContent != null
            && (draft.LastContent == null || draft.LastContent.MarkdownContent != nextMarkdown))
        {
            await _noteDao.CreateContentAsync(draft, nextMarkdown, now);
        }

        SetUpdatedBy(draft, user, now);
        await _noteDao.SaveNodeAsync(draft);

        return draft;
    }

    public async Task DeleteDraftAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId)
    {
        var draft = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureDraft(draft);

        await DeleteAttachmentsAsync(user, draft);
        SetUpdatedBy(draft, user);
        await _noteDao.DeleteDraftAsync(draft);
    }

    public async Task<NoteNodeEntity> PromoteDraftAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string title,
        NoteVisibility visibility,
        Guid? parentId,
        int? sortOrder,
        string? markdownContent
    )
    {
        var draft = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureDraft(draft);
        await EnsureCanUseVisibilityAsync(workspace, user, visibility);

        var parent = await GetValidParentAsync(workspace, user, parentId);
        EnsureMatchingParentVisibility(parent, visibility);

        var now = DateTime.UtcNow;
        if (markdownContent != null)
        {
            var nextMarkdown = NormalizeMarkdown(markdownContent);
            if (draft.LastContent == null || draft.LastContent.MarkdownContent != nextMarkdown)
            {
                await _noteDao.CreateContentAsync(draft, nextMarkdown, now);
            }
        }

        draft.IsDraft = false;
        draft.Visibility = visibility;
        draft.Parent = parent;
        draft.Title = NormalizeTitle(title);
        draft.SortOrder = await ResolveSortOrderAsync(workspace, parent, sortOrder);
        SetUpdatedBy(draft, user, now);

        await _noteDao.SaveNodeAsync(draft);

        return draft;
    }

    public async Task<ICollection<NoteNodeEntity>> GetLinkedNotesAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        NoteLinkEntityType entityType,
        Guid entityId
    )
    {
        await EnsureLinkedEntityExistsAsync(workspace, entityType, entityId);

        var links = await _noteDao.GetLinksByEntityAsync(workspace, entityType, entityId);
        var notes = links
            .Select(item => item.NoteNode)
            .Where(item => item.Type == NoteNodeType.Document && !item.IsDraft && item.ArchivedAt == null)
            .DistinctBy(item => item.Id)
            .ToList();

        return await GetAvailableNotesAsync(user, notes, AccessLevel.Read);
    }

    public async Task<NoteLinkEntity> CreateLinkAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        NoteLinkEntityType entityType,
        Guid entityId
    )
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureDocument(note);
        EnsureNotDraft(note);
        await EnsureLinkedEntityExistsAsync(workspace, entityType, entityId);

        if (await _noteDao.IsLinkExistsAsync(note, entityType, entityId))
        {
            throw new RecordIsExistsException("Note link already exists");
        }

        return await _noteDao.CreateLinkAsync(
            workspace,
            note,
            user,
            entityType,
            entityId
        );
    }

    public async Task DeleteLinkAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId, Guid linkId)
    {
        var note = await GetNoteAsync(workspace, user, noteId, AccessLevel.Write);
        EnsureDocument(note);

        var link = await _noteDao.GetLinkByIdAsync(workspace, note, linkId);
        RecordNotFoundException.ThrowIfNull(link);
        await _noteDao.DeleteLinkAsync(link);
    }

    private async Task DeleteAttachmentsAsync(UserEntity user, NoteNodeEntity note)
    {
        foreach (var attachment in note.Attachments.ToList())
        {
            note.Attachments.Remove(attachment);
            await _fileStorage.DeleteFile(user, attachment.Id);
        }
    }

    private async Task<NoteNodeEntity> GetNoteAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        AccessLevel accessLevel
    )
    {
        var note = await _noteDao.GetNodeByIdAsync(workspace, noteId);
        RecordNotFoundException.ThrowIfNull(note);
        await _securityManager.CheckAccess(accessLevel, user, note);
        return note;
    }

    private async Task<ICollection<NoteNodeEntity>> GetAvailableNotesAsync(
        UserEntity user,
        IEnumerable<NoteNodeEntity> notes,
        AccessLevel accessLevel
    )
    {
        var result = new List<NoteNodeEntity>();
        foreach (var note in notes)
        {
            if (await _securityManager.HasAccess(accessLevel, user, note))
            {
                result.Add(note);
            }
        }

        return result;
    }

    private async Task<NoteNodeEntity?> GetValidParentAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid? parentId
    )
    {
        if (!parentId.HasValue)
        {
            return null;
        }

        var parent = await GetNoteAsync(workspace, user, parentId.Value, AccessLevel.Write);
        if (parent.Type != NoteNodeType.Folder)
        {
            throw new DataValidationException("Parent note must be a folder");
        }

        return parent;
    }

    private async Task EnsureCanUseVisibilityAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        NoteVisibility visibility
    )
    {
        if (visibility == NoteVisibility.Workspace
            && (workspace.Mode == WorkspaceMode.Solo || !await _securityManager.HasAccess(AccessLevel.Write, user, workspace)))
        {
            throw new HasNoAccessException();
        }
    }

    private static void EnsureMatchingParentVisibility(
        NoteNodeEntity? parent,
        NoteVisibility visibility
    )
    {
        if (parent != null && parent.Visibility != visibility)
        {
            throw new DataValidationException("Child note visibility must match parent visibility");
        }
    }

    private async Task EnsureFolderIsNotMovedIntoItselfOrDescendantAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity folder,
        NoteNodeEntity parent
    )
    {
        if (folder.Id == parent.Id)
        {
            throw new DataValidationException("Folder can not be moved into itself");
        }

        var allNodes = await _noteDao.GetWorkspaceNodesAsync(workspace);
        var descendantIds = GetNodeWithDescendants(folder, allNodes)
            .Select(item => item.Id)
            .ToHashSet();
        if (descendantIds.Contains(parent.Id))
        {
            throw new DataValidationException("Folder can not be moved into its descendant");
        }
    }

    private async Task EnsureLinkedEntityExistsAsync(
        WorkspaceEntity workspace,
        NoteLinkEntityType entityType,
        Guid entityId
    )
    {
        switch (entityType)
        {
            case NoteLinkEntityType.Client:
                var client = await _clientDao.GetById(entityId, workspace);
                RecordNotFoundException.ThrowIfNull(client);
                break;
            case NoteLinkEntityType.Project:
                var project = await _projectDao.GetById(entityId);
                if (project == null || project.Client.Workspace.Id != workspace.Id)
                {
                    throw new RecordNotFoundException();
                }
                break;
            case NoteLinkEntityType.Task:
                var task = await _taskDao.GetById(entityId);
                if (task == null || task.Workspace.Id != workspace.Id)
                {
                    throw new RecordNotFoundException();
                }
                break;
            default:
                throw new DataValidationException("Unsupported linked entity type");
        }
    }

    private async Task<int> ResolveSortOrderAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity? parent,
        int? sortOrder
    )
    {
        return sortOrder ?? await _noteDao.GetNextSortOrderAsync(workspace, parent);
    }

    private static void EnsureDocument(NoteNodeEntity note)
    {
        if (note.Type != NoteNodeType.Document)
        {
            throw new DataValidationException("Note must be a document");
        }
    }

    private static void EnsureDraft(NoteNodeEntity note)
    {
        if (note.Type != NoteNodeType.Document || !note.IsDraft)
        {
            throw new DataValidationException("Note must be a draft");
        }
    }

    private static void EnsureNotDraft(NoteNodeEntity note)
    {
        if (note.IsDraft)
        {
            throw new DataValidationException("Draft notes cannot use this action");
        }
    }

    private async Task<string> GenerateDraftTitleAsync(
        UserEntity user,
        string? markdownContent
    )
    {
        var derivedTitle = TryDeriveTitleFromMarkdown(markdownContent);
        if (!string.IsNullOrWhiteSpace(derivedTitle))
        {
            return derivedTitle;
        }

        var nextNumber = await _noteDao.GetNextDraftNumberAsync(user);
        return $"Draft {nextNumber}";
    }

    private static string? TryDeriveTitleFromMarkdown(string? markdownContent)
    {
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            return null;
        }

        var lines = markdownContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim().TrimStart('#', '>', '-', '*', '+', '`', ' ');
            if (line.StartsWith("[ ] ", StringComparison.Ordinal)
                || line.StartsWith("[x] ", StringComparison.OrdinalIgnoreCase))
            {
                line = line[4..].TrimStart();
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var words = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var title = string.Join(' ', words.Take(4));
            if (title.Length > 60)
            {
                title = title[..60].TrimEnd();
            }

            return string.IsNullOrWhiteSpace(title) ? null : title;
        }

        return null;
    }

    private static string NormalizeTitle(string title)
    {
        var normalizedTitle = title.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new DataValidationException("Title is required");
        }

        if (normalizedTitle.Length > TitleMaxLength)
        {
            throw new DataValidationException("Title is too long");
        }

        return normalizedTitle;
    }

    private static string NormalizeMarkdown(string? markdownContent)
    {
        var normalizedMarkdown = markdownContent ?? string.Empty;
        if (normalizedMarkdown.Length > MarkdownMaxLength)
        {
            throw new DataValidationException("Markdown content is too long");
        }

        return normalizedMarkdown;
    }

    private static void EnsureNoDuplicateLinks(IEnumerable<NoteLinkTargetDto> links)
    {
        var duplicateLink = links
            .GroupBy(item => new { item.EntityType, item.EntityId })
            .Any(item => item.Count() > 1);
        if (duplicateLink)
        {
            throw new RecordIsExistsException("Note link already exists");
        }
    }

    private static void SetUpdatedBy(NoteNodeEntity node, UserEntity user, DateTime? now = null)
    {
        node.UpdatedByUser = user;
        node.UpdatedAt = now ?? DateTime.UtcNow;
    }

    private static ICollection<NoteNodeEntity> GetNodeWithDescendants(
        NoteNodeEntity node,
        ICollection<NoteNodeEntity> allNodes
    )
    {
        var result = new List<NoteNodeEntity> { node };
        var pendingIds = new Queue<Guid>();
        pendingIds.Enqueue(node.Id);

        while (pendingIds.TryDequeue(out var parentId))
        {
            var children = allNodes.Where(item => item.Parent?.Id == parentId).ToList();
            foreach (var child in children)
            {
                result.Add(child);
                pendingIds.Enqueue(child.Id);
            }
        }

        return result;
    }
}
