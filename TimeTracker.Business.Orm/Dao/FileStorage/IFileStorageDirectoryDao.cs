using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.FileStorage;

public interface IFileStorageDirectoryDao: IDomainService
{
    Task<FileStorageDirectoryEntity> CreateOrUpdate(
        FileStorageBucketEntity bucket,
        string name,
        FileStorageDirectoryEntity? parent = null
    );
}
