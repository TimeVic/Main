using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Notes;

public class NoteDao : INoteDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public NoteDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<NoteNodeEntity> CreateNodeAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity? parent,
        UserEntity createdByUser,
        NoteNodeType type,
        string title,
        string? markdownContent,
        NoteVisibility visibility,
        int sortOrder
    )
    {
        var now = DateTime.UtcNow;
        var node = new NoteNodeEntity
        {
            Workspace = workspace,
            Parent = parent,
            Type = type,
            Title = title,
            MarkdownContent = markdownContent,
            Visibility = visibility,
            SortOrder = sortOrder,
            CreatedByUser = createdByUser,
            UpdatedByUser = createdByUser,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _sessionProvider.CurrentSession.SaveAsync(node);
        return node;
    }

    public async Task<NoteNodeEntity?> GetNodeByIdAsync(
        WorkspaceEntity workspace,
        Guid? noteId,
        bool isIncludeArchived = false
    )
    {
        if (!noteId.HasValue)
        {
            return null;
        }

        return await _sessionProvider.CurrentSession.Query<NoteNodeEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => item.Id == noteId.Value)
            .Where(item => isIncludeArchived || item.ArchivedAt == null)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<NoteNodeEntity>> GetTreeAsync(WorkspaceEntity workspace, bool isIncludeArchived)
    {
        var items = await _sessionProvider.CurrentSession.Query<NoteNodeEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => isIncludeArchived || item.ArchivedAt == null)
            .Fetch(item => item.Parent)
            .Fetch(item => item.CreatedByUser)
            .ToListAsync();

        return items
            .OrderBy(item => item.Parent == null ? Guid.Empty : item.Parent.Id)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Title)
            .ToList();
    }

    public async Task<int> GetNextSortOrderAsync(WorkspaceEntity workspace, NoteNodeEntity? parent)
    {
        var query = _sessionProvider.CurrentSession.Query<NoteNodeEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => item.ArchivedAt == null);

        query = parent == null
            ? query.Where(item => item.Parent == null)
            : query.Where(item => item.Parent != null && item.Parent.Id == parent.Id);

        var maxSortOrder = await query
            .Select(item => (int?)item.SortOrder)
            .MaxAsync();

        return (maxSortOrder ?? 0) + 1000;
    }

    public async Task SaveNodeAsync(NoteNodeEntity node)
    {
        await _sessionProvider.CurrentSession.SaveOrUpdateAsync(node);
    }

    public async Task<ICollection<NoteNodeEntity>> GetWorkspaceNodesAsync(WorkspaceEntity workspace)
    {
        return await _sessionProvider.CurrentSession.Query<NoteNodeEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .ToListAsync();
    }

    public async Task<NoteLinkEntity> CreateLinkAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity noteNode,
        UserEntity createdByUser,
        NoteLinkEntityType entityType,
        Guid entityId
    )
    {
        var link = new NoteLinkEntity
        {
            Workspace = workspace,
            NoteNode = noteNode,
            CreatedByUser = createdByUser,
            EntityType = entityType,
            EntityId = entityId,
            CreatedAt = DateTime.UtcNow
        };

        noteNode.Links.Add(link);
        await _sessionProvider.CurrentSession.SaveAsync(link);
        return link;
    }

    public async Task<NoteLinkEntity?> GetLinkByIdAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity noteNode,
        Guid linkId
    )
    {
        return await _sessionProvider.CurrentSession.Query<NoteLinkEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => item.NoteNode.Id == noteNode.Id)
            .Where(item => item.Id == linkId)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<NoteLinkEntity>> GetLinksByNoteAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity noteNode
    )
    {
        return await _sessionProvider.CurrentSession.Query<NoteLinkEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => item.NoteNode.Id == noteNode.Id)
            .ToListAsync();
    }

    public async Task<ICollection<NoteLinkEntity>> GetLinksByEntityAsync(
        WorkspaceEntity workspace,
        NoteLinkEntityType entityType,
        Guid entityId
    )
    {
        return await _sessionProvider.CurrentSession.Query<NoteLinkEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => item.EntityType == entityType)
            .Where(item => item.EntityId == entityId)
            .ToListAsync();
    }

    public async Task<bool> IsLinkExistsAsync(
        NoteNodeEntity noteNode,
        NoteLinkEntityType entityType,
        Guid entityId
    )
    {
        return await _sessionProvider.CurrentSession.Query<NoteLinkEntity>()
            .AnyAsync(item =>
                item.NoteNode.Id == noteNode.Id
                && item.EntityType == entityType
                && item.EntityId == entityId
            );
    }

    public async Task DeleteLinkAsync(NoteLinkEntity link)
    {
        await _sessionProvider.CurrentSession.DeleteAsync(link);
    }
}
