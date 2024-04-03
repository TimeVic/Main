using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.FileStorage;

public interface IFileStorageAccessKeyDao: IDomainService
{
    Task<FileStorageAccessKeyEntity> Create(UserEntity user, DateTime? expirationTime = null);
    
    Task<FileStorageAccessKeyEntity?> GetByKey(string accessKey, string secretKey);
}
