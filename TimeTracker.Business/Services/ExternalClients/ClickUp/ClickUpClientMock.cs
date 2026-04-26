using System.Globalization;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public class ClickUpClientMock: IClickUpClient
{
    public ICollection<TimeEntryEntity> SentTimeEntries = new List<TimeEntryEntity>();
    
    public bool IsSent => SentTimeEntries.Count > 0;

    private readonly ITaskDao _taskDao;
    
    public ClickUpClientMock(ITaskDao taskDao)
    {
        _taskDao = taskDao;
    }

    public void Reset()
    {
        SentTimeEntries.Clear();
    }

    public async Task<SynchronizedTimeEntryDto?> SetTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        SentTimeEntries.Add(timeEntry);
        return new SynchronizedTimeEntryDto()
        {
            Id = "123",
            AdditionalDescription = "Test description"
        };
    }

    public Task<bool> IsFillTimeEntryDescriptionFromTaskTitle(TimeEntryEntity timeEntry)
    {
        return Task.FromResult(true);
    }

    public async Task<GetTaskResponseDto?> GetTaskAsync(TimeEntryEntity timeEntry, string externalTaskId)
    {
        return null;
    }

    public Task<ExternalTaskInfoDto?> GetTaskInfoAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string externalTaskId
    )
    {
        return Task.FromResult<ExternalTaskInfoDto?>(null);
    }

    public bool IsCorrectTaskId(TimeEntryEntity timeEntry)
    {
        return true;
    }
    
    public bool IsCorrectTaskId(string externalTaskId)
    {
        return true;
    }

    public Task<bool> DeleteTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        SentTimeEntries.Add(timeEntry);
        return Task.FromResult(true);
    }

    public Task<bool> IsValidClientSettings(WorkspaceEntity workspace, UserEntity user)
    {
        return Task.FromResult<bool>(true);
    }

    public async Task<TaskEntity> SetTimeEntryTaskAsync(
        TimeEntryEntity timeEntry,
        TaskListEntity taskList,
        string externalTaskId
    )
    {
        timeEntry.Task = await SetTimeEntryTaskAsync(taskList, timeEntry.User, externalTaskId);
        return timeEntry.Task;
    }

    public async Task<TaskEntity> SetTimeEntryTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string externalTaskId
    )
    {
        var task = await _taskDao.AddTaskAsync(taskList, user, "Test task");
        task.ExternalTaskId = externalTaskId;
        return task;
    }
}
