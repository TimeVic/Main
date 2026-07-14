using Domain.Abstractions;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Storage;

public class FileStorageRelationshipService: IFileStorageRelationshipService
{
    private readonly IUserDao _userDao;
    private readonly ITaskDao _taskDao;
    private readonly ITaskCommentDao _taskCommentDao;
    private readonly INoteDao _noteDao;
    private readonly IDbSessionProvider _sessionProvider;

    public FileStorageRelationshipService(
        IUserDao userDao,
        ITaskDao taskDao,
        ITaskCommentDao taskCommentDao,
        INoteDao noteDao,
        IDbSessionProvider sessionProvider
    )
    {
        _userDao = userDao;
        _taskDao = taskDao;
        _taskCommentDao = taskCommentDao;
        _noteDao = noteDao;
        _sessionProvider = sessionProvider;
    }
    
    public async Task<IEntity> GetFileRelationship(
        Guid entityId,
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
        if (entityType == StorageEntityType.NoteNode)
        {
            entity = await _noteDao.GetNodeByIdAsync(entityId);
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
        if (entity is UserEntity userEntity)
        {
            userEntity.Avatars.Add(file);
        }
        if (entity is TaskEntity taskEntity)
        {
            taskEntity.Attachments.Add(file);
        }
        if (entity is TaskCommentEntity taskCommentEntity)
        {
            taskCommentEntity.Attachments.Add(file);
        }
        if (entity is NoteNodeEntity noteNodeEntity)
        {
            noteNodeEntity.Attachments.Add(file);
        }
        await _sessionProvider.CurrentSession.SaveAsync(entity);
    }
}
