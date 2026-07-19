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
        var query = _sessionProvider.CurrentSession.Query<StoredFileEntity>();
        if (entityType == StorageEntityType.Task)
        {
            query = query.Where(item => item.Tasks.Any(task => task.Id == entityId));
        }
        if (entityType == StorageEntityType.User)
        {
            query = query.Where(item => item.Users.Any(user => user.Id == entityId));
        }
        return await query
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
    }
}
