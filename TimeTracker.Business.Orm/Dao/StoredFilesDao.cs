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
    
    /*
     * Only for testing purposes
     */
    public async Task MarkAsUploadedAllPending()
    {
        await _sessionProvider.CurrentSession.CreateQuery(@"update StoredFileEntity set Status = 2")
            .ExecuteUpdateAsync();
    }
}
