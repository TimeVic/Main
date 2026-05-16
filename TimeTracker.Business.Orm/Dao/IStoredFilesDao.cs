using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IStoredFilesDao: IDomainService
{
    Task<ICollection<StoredFileEntity>> GetListByEntity(Guid entityId, StorageEntityType entityType);
}
