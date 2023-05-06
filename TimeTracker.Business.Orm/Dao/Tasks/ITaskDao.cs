using Domain.Abstractions;
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
        bool isArchived = false
    );

    Task<TaskEntity?> GetById(long taskListId);

    Task<ListDto<TaskEntity>> GetList(
        TaskListEntity taskList,
        GetTasksFilterDto? filter = null
    );
}
