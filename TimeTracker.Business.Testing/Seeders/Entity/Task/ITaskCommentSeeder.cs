using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Testing.Seeders.Entity.Task;

public interface ITaskCommentSeeder: IDomainService
{
    Task<ICollection<TaskCommentEntity>> CreateSeveralAsync(
        TaskEntity taskEntity,
        int count = 1,
        UserEntity? user = null
    );

    Task<TaskCommentEntity> CreateAsync(
        TaskEntity? task = null,
        UserEntity? user = null,
        ICollection<UserEntity>? watchers = null
    );
}
