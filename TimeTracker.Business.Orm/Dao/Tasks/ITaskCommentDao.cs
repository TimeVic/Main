using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskCommentDao: IDomainService
{
    Task<TaskCommentEntity?> GetById(long taskCommentId);

    Task<TaskCommentEntity> AddAsync(
        TaskEntity task,
        UserEntity user,
        string comment,
        ICollection<UserEntity>? watchers = null
    );

    Task<TaskCommentEntity> UpdateAsync(
        TaskCommentEntity taskComment,
        string comment,
        ICollection<UserEntity>? watchers = null
    );

    Task<ListDto<TaskCommentEntity>> GetList(
        TaskEntity task,
        int page
    );

    Task DeleteAsync(TaskCommentEntity taskComment);
}
