using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Dto.Notes;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Services.Entity;

/// <summary>
/// Domain service handling note node, draft, link, and attachment business operations and validations.
/// </summary>
public interface INoteService : IDomainService
{
    Task<NoteTreeResultDto> GetTreeAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        bool isIncludeArchived,
        NoteVisibility visibility
    );

    Task<NoteNodeEntity> GetDocumentAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId);

    Task<NoteContentEntity> GetContentAsync(WorkspaceEntity workspace, UserEntity user, Guid contentId);

    Task<ICollection<NoteNodeHistoryEntity>> GetHistoryAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId);

    Task<NoteNodeEntity> CreateFolderAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string title,
        NoteVisibility visibility,
        Guid? parentId,
        int? sortOrder
    );

    Task<NoteNodeEntity> CreateDocumentAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string title,
        NoteVisibility visibility,
        Guid? parentId,
        int? sortOrder,
        ICollection<NoteLinkTargetDto>? links = null
    );

    Task<NoteNodeEntity> UpdateDocumentAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string title
    );

    Task<NoteContentEntity> UpdateContentAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string? markdownContent
    );

    Task<NoteNodeEntity> RenameNodeAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string title
    );

    Task<NoteNodeEntity> MoveNodeAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        Guid? parentId,
        int? sortOrder
    );

    Task<NoteNodeEntity> ArchiveNodeAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId);

    Task<NoteNodeEntity> CreateDraftAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string? title,
        string? markdownContent
    );

    Task<NoteNodeEntity> UpdateDraftAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string? title,
        string? markdownContent
    );

    Task DeleteDraftAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId);

    Task<NoteNodeEntity> PromoteDraftAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        string title,
        NoteVisibility visibility,
        Guid? parentId,
        int? sortOrder,
        string? markdownContent
    );

    Task<ICollection<NoteNodeEntity>> GetLinkedNotesAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        NoteLinkEntityType entityType,
        Guid entityId
    );

    Task<NoteLinkEntity> CreateLinkAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        NoteLinkEntityType entityType,
        Guid entityId
    );

    Task DeleteLinkAsync(WorkspaceEntity workspace, UserEntity user, Guid noteId, Guid linkId);
}
