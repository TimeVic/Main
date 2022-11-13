using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Integrations;

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
}
