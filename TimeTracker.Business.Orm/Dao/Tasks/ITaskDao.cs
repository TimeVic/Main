using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Orm.Entities;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskDao: IDomainService
{
    Task<TaskEntity> AddTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string title,
        string? description = null,
        DateTime? notificationTime = null,
        TaskStatus status = TaskStatus.Backlog,
        TaskPriority priority = TaskPriority.Low,
        bool isArchived = false
    );

    Task<TaskEntity?> GetById(long taskListId);

    Task<ListDto<TaskEntity>> GetList(
        WorkspaceEntity? workspace = null,
        TaskListEntity? taskList = null,
        GetTasksFilterDto? filter = null
    );
}
