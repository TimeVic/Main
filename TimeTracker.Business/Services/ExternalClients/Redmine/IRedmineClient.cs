using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.Dto;
using TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

namespace TimeTracker.Business.Services.ExternalClients.Redmine;

public interface IRedmineClient: IDomainService
{
    Task<SynchronizedTimeEntryDto?> SetTimeEntryAsync(TimeEntryEntity timeEntry);

    Task<bool> IsFillTimeEntryDescriptionFromTaskTitle(TimeEntryEntity timeEntry);

    Task<ExternalTaskInfoDto?> GetTaskInfoAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string externalTaskId
    );
    
    Task<bool> DeleteTimeEntryAsync(TimeEntryEntity timeEntry);

    bool IsCorrectTaskId(TimeEntryEntity timeEntry);

    Task<bool> IsValidClientSettings(WorkspaceEntity workspace, UserEntity user);
    
    Task<TaskEntity> SetTimeEntryTaskAsync(
        TimeEntryEntity timeEntry,
        TaskListEntity taskListEntity,
        string externalTaskId
    );
}
