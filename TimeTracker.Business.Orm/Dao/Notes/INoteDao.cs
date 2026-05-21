using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Notes;

public interface INoteDao : IDomainService
{
    Task<NoteNodeEntity> CreateNodeAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity? parent,
        UserEntity createdByUser,
        NoteNodeType type,
        string title,
        string? markdownContent,
        NoteVisibility visibility,
        int sortOrder
    );

    Task<NoteNodeEntity?> GetNodeByIdAsync(WorkspaceEntity workspace, Guid? noteId, bool isIncludeArchived = false);

    Task<ICollection<NoteNodeEntity>> GetTreeAsync(WorkspaceEntity workspace, bool isIncludeArchived);

    Task<int> GetNextSortOrderAsync(WorkspaceEntity workspace, NoteNodeEntity? parent);

    Task SaveNodeAsync(NoteNodeEntity node);

    Task<ICollection<NoteNodeEntity>> GetWorkspaceNodesAsync(WorkspaceEntity workspace);

    Task<NoteLinkEntity> CreateLinkAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity noteNode,
        UserEntity createdByUser,
        NoteLinkEntityType entityType,
        Guid entityId
    );

    Task<NoteLinkEntity?> GetLinkByIdAsync(WorkspaceEntity workspace, NoteNodeEntity noteNode, Guid linkId);

    Task<ICollection<NoteLinkEntity>> GetLinksByNoteAsync(WorkspaceEntity workspace, NoteNodeEntity noteNode);

    Task<ICollection<NoteLinkEntity>> GetLinksByEntityAsync(
        WorkspaceEntity workspace,
        NoteLinkEntityType entityType,
        Guid entityId
    );

    Task<bool> IsLinkExistsAsync(
        NoteNodeEntity noteNode,
        NoteLinkEntityType entityType,
        Guid entityId
    );

    Task DeleteLinkAsync(NoteLinkEntity link);
}
