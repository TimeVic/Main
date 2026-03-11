using NHibernate.Criterion;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public class TaskCommentDao: ITaskCommentDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public TaskCommentDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<TaskCommentEntity?> GetById(Guid taskCommentId)
    {
        return await _sessionProvider.CurrentSession.GetAsync<TaskCommentEntity>(taskCommentId);
    }

    public async Task<TaskCommentEntity> AddAsync(
        TaskEntity task,
        UserEntity user,
        string comment,
        ICollection<UserEntity>? watchers = null
    )
    {
        var taskComment = new TaskCommentEntity()
        {
            Task = task,
            User = user,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        if (watchers != null)
        {
            foreach (var userEntity in watchers)
            {
                taskComment.Watchers.Add(userEntity);
            }
        }

        await _sessionProvider.CurrentSession.SaveAsync(taskComment);
        return taskComment;
    }
    
    public async Task<TaskCommentEntity> UpdateAsync(
        TaskCommentEntity taskComment,
        string comment,
        ICollection<UserEntity>? watchers = null
    )
    {
        taskComment.Comment = comment;
        taskComment.UpdatedAt = DateTime.UtcNow;
        taskComment.Watchers.Clear();
        if (watchers != null)
        {
            foreach (var userEntity in watchers)
            {
                taskComment.Watchers.Add(userEntity);
            }
        }

        await _sessionProvider.CurrentSession.SaveAsync(taskComment);
        return taskComment;
    }
    
    public async Task DeleteAsync(TaskCommentEntity taskComment)
    {
        taskComment.IsArchived = true;
        await _sessionProvider.CurrentSession.SaveAsync(taskComment);
    }
    
    public async Task<ListDto<TaskCommentEntity>> GetList(
        TaskEntity task,
        int page
    )
    {
        TaskListEntity taskAlias = null;
        UserEntity userAlias = null;
        var query = _sessionProvider.CurrentSession.QueryOver<TaskCommentEntity>()
            .Inner.JoinAlias(item => item.Task, () => taskAlias)
            .Inner.JoinAlias(item => item.User, () => userAlias)
            .Where(() => taskAlias!.Id == task.Id)
            .Where(item => item.IsArchived == false);

        var offset = PaginationUtils.CalculateOffset(page);
        var items = await query
            .OrderBy(item => item.CreatedAt).Desc()
            .Skip(offset)
            .Take(GlobalConstants.ListPageSize)
            .ListAsync<TaskCommentEntity>();
        return new ListDto<TaskCommentEntity>(
            items,
            await query.RowCountAsync()
        );
    }
}
