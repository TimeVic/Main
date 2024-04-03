using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Business.Orm.Dao.FileStorage;

public class FileStorageFileDao: IFileStorageFileDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public FileStorageFileDao(
        IDbSessionProvider sessionProvider
    )
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<FileStorageFileEntity?> GetByExternalId(string bucket, string externalId)
    {
        return await _sessionProvider.CurrentSession.Query<FileStorageFileEntity>()
            .Where(item => item.ExternalId == externalId && item.Bucket!.Name == bucket)
            .FirstOrDefaultAsync();
    }
}
