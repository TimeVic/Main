using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IStoredFilesDao: IDomainService
{
    Task<StoredFileEntity?> GetFirstToUpload();

    Task MarkAsUploadedAllPending();
}
