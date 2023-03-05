using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IStoredFilesDao: IDomainService
{
    Task<StoredFileEntity?> GetFirstToUpload();

    Task MarkAsUploadedAllPending();

    Task<ICollection<StoredFileEntity>> GetListByEntity(long entityId, StorageEntityType entityType);
}
