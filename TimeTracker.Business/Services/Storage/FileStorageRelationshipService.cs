using Domain.Abstractions;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public class FileStorageRelationshipService: IFileStorageRelationshipService
{
    private readonly IUserDao _userDao;
    private readonly ITaskDao _taskDao;
    private readonly ITaskCommentDao _taskCommentDao;
    private readonly IDbSessionProvider _sessionProvider;

    public FileStorageRelationshipService(
        IUserDao userDao,
        ITaskDao taskDao,
        ITaskCommentDao taskCommentDao,
        IDbSessionProvider sessionProvider
    )
    {
        _userDao = userDao;
        _taskDao = taskDao;
        _taskCommentDao = taskCommentDao;
        _sessionProvider = sessionProvider;
    }
    
    public async Task<IEntity> GetFileRelationship(
        long entityId,
        StorageEntityType entityType
    )
    {
        IEntity? entity = null;
        if (entityType == StorageEntityType.User)
        {
            entity = await _userDao.GetById(entityId);
        }
        if (entityType == StorageEntityType.Task)
        {
            entity = await _taskDao.GetById(entityId);
        }
        if (entityType == StorageEntityType.TaskComment)
        {
            entity = await _taskCommentDao.GetById(entityId);
        }
        if (entity == null)
        {
            throw new RecordNotFoundException("EntityUid is incorrect");
        }

        return entity;
    }
    
    public async Task AddFileRelationship<TEntity>(
        TEntity entity,
        StoredFileEntity file
    ) where TEntity: IEntity
    {
        if (entity is UserEntity)
        {
            // TODO: Add relationship
        }
        if (entity is TaskEntity taskEntity)
        {
            taskEntity.Attachments.Add(file);
        }
        if (entity is TaskCommentEntity taskCommentEntity)
        {
            taskCommentEntity.Attachments.Add(file);
        }
        await _sessionProvider.CurrentSession.SaveAsync(entity);
    }
}
