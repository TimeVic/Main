using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Workspace;

public interface IWorkspaceSettingsDao: IDomainService
{
    Task<WorkspaceSettingsClickUpEntity> SetClickUpAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        string? securityKey,
        string? teamId,
        bool isCustomTaskIds,
        bool isFillTimeEntryWithTaskDetails = true
    );

    Task<WorkspaceSettingsRedmineEntity> SetRedmineAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        string? redmineUrl,
        string? apiKey,
        long? redmineUserId,
        long? redmineActivityId
    );

    Task<WorkspaceSettingsJiraEntity> SetJiraAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        string? url,
        string? apiKey,
        string? userName,
        bool isFillTimeEntryWithTaskDetails = true
    );
}
