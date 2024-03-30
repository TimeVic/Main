using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.FileStorage;

public interface IFileStorageBucketDao: IDomainService
{
    Task<FileStorageBucketEntity?> GetBy(
        long? id = null,
        string? name = null
    );

    Task<FileStorageBucketEntity> Create(UserEntity user, string name);
}
