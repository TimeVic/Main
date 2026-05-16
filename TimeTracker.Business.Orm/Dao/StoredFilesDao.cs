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
        TaskEntity taskAlias = null!;
        var query = _sessionProvider.CurrentSession.QueryOver<StoredFileEntity>()
            .Left.JoinAlias(item => item.Tasks, () => taskAlias);
        if (entityType == StorageEntityType.Task)
        {
            query = query.Where(() => taskAlias!.Id == entityId);
        }
        query = query.OrderBy(item => item.CreatedAt).Desc;
        return await query.ListAsync();
    }
}
