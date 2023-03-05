using NHibernate.Criterion;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class StoredFilesDao: IStoredFilesDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public StoredFilesDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<StoredFileEntity?> GetFirstToUpload()
    {
        var storedFile = await _sessionProvider.CurrentSession.Query<StoredFileEntity>()
            .Where(item => item.Status == StoredFileStatus.Pending)
            .OrderBy(item => item.CreateTime)
            .FirstOrDefaultAsync();
        if (storedFile == null)
        {
            return null;
        }
        storedFile.Status = StoredFileStatus.Uploading;
        await _sessionProvider.CurrentSession.SaveAsync(storedFile);
        return storedFile;
    }
    
    public async Task<ICollection<StoredFileEntity>> GetListByEntity(long entityId, StorageEntityType entityType)
    {
        TaskEntity taskAlias = null;
        UserEntity userAlias = null;
        var query = _sessionProvider.CurrentSession.QueryOver<StoredFileEntity>()
            .Left.JoinAlias(item => item.Tasks, () => taskAlias);
        if (entityType == StorageEntityType.Task)
        {
            query = query.Where(() => taskAlias.Id == entityId);
        }
        query = query.OrderBy(item => item.CreateTime).Desc;
        return await query.ListAsync();
    }
    
    /*
     * Only for testing purposes
     */
    public async Task MarkAsUploadedAllPending()
    {
        await _sessionProvider.CurrentSession.CreateQuery(@"update StoredFileEntity set Status = 2")
            .ExecuteUpdateAsync();
    }
}
