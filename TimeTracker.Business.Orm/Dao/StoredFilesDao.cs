using NHibernate.Criterion;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao;

public class StoredFilesDao: IStoredFilesDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public StoredFilesDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<ICollection<StoredFileEntity>> GetListByEntity(Guid entityId, StorageEntityType entityType)
    {
        var session = _sessionProvider.CurrentSession;
        
        if (entityType == StorageEntityType.Task)
        {
            return await session.Query<StoredFileEntity>()
                .Where(f => f.Tasks.Any(t => t.Id == entityId))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
        
        if (entityType == StorageEntityType.User)
        {
            return await session.Query<StoredFileEntity>()
                .Where(f => f.Users.Any(u => u.Id == entityId))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
        
        return new List<StoredFileEntity>();
    }
}
