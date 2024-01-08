using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskDao: IDomainService
{
    Task<TaskEntity> AddTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string title,
        string? description = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        TaskStatus status = TaskStatus.Backlog,
        TaskPriority priority = TaskPriority.Low,
        bool isArchived = false
    );

    Task<TaskEntity> UpdateTaskAsync(
        TaskEntity task,
        TaskListEntity taskList,
        UserEntity user,
        string title,
        string? description = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        TaskStatus status = TaskStatus.Backlog,
        TaskPriority priority = TaskPriority.Low,
        bool isArchived = false,
        IEnumerable<TagEntity>? tags = null,
        DateTime? reminderTime = null
    );
    
    Task<TaskEntity?> GetById(long taskListId);

    Task<TaskEntity?> GetByWorkspaceTaskId(
        long workspaceId,
        long workspaceTaskId
    );
    
    Task<ListDto<TaskEntity>> GetList(
        WorkspaceEntity? workspace = null,
        TaskListEntity? taskList = null,
        GetTasksFilterDto? filter = null
    );

    Task UpdatePositions(WorkspaceEntity workspace, IDictionary<long, int> items);

    Task<ICollection<TaskEntity>> GetTasksToRemind();
}
