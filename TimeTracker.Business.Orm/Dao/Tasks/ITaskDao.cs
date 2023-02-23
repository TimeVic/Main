using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskDao: IDomainService
{
    Task<TaskEntity> AddTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string title,
        string? description = null,
        DateTime? notificationTime = null,
        bool isDone = false,
        bool isArchived = false
    );

    Task<TaskEntity?> GetById(long taskListId);

    Task<ListDto<TaskEntity>> GetList(
        TaskListEntity taskList,
        int page,
        GetTasksFilterDto? filter = null
    );
}
