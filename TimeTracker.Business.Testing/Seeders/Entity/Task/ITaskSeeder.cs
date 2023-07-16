using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Business.Testing.Seeders.Entity.Task;

public interface ITaskSeeder: IDomainService
{
    Task<ICollection<TaskEntity>> CreateSeveralAsync(
        TaskListEntity taskList,
        int count = 1,
        UserEntity? user = null
    );

    Task<TaskEntity> CreateAsync(TaskListEntity? taskList = null, UserEntity? user = null);
}
