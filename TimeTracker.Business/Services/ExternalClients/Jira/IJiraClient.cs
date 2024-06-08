using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.Jira.Dto;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients.Jira;

public interface IJiraClient
{
    Task<SynchronizedTimeEntryDto?> SetTimeEntryAsync(TimeEntryEntity timeEntry);

    Task<bool> IsFillTimeEntryDescriptionFromTaskTitle(TimeEntryEntity timeEntry);

    Task<GetTaskResponseDto?> GetTaskAsync(TimeEntryEntity timeEntry, string externalTaskId);

    bool IsCorrectTaskId(TimeEntryEntity timeEntry);
    
    bool IsCorrectTaskId(string externalTaskId);

    Task<bool> DeleteTimeEntryAsync(TimeEntryEntity timeEntry);

    Task<bool> IsValidClientSettings(WorkspaceEntity workspace, UserEntity user);

    Task<TaskEntity> SetTimeEntryTaskAsync(
        TimeEntryEntity timeEntry,
        TaskListEntity taskListEntity,
        string externalTaskId
    );

    Task<TaskEntity> SetTimeEntryTaskAsync(
        TaskListEntity taskListEntity,
        UserEntity user,
        string externalTaskId
    );
}
