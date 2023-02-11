using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface ITaskListSeeder: IDomainService
{
    Task<ICollection<TaskListEntity>> CreateSeveralAsync(ProjectEntity project, int count = 1);

    Task<TaskListEntity> CreateAsync(ProjectEntity project);
}
