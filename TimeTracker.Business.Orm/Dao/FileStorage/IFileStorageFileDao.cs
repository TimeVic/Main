using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Business.Orm.Dao.FileStorage;

public interface IFileStorageFileDao: IDomainService
{
    Task<FileStorageFileEntity?> GetByExternalId(string bucket, string externalId);

    Task<FileStorageFileEntity?> GetByName(string fileName, FileStorageDirectoryEntity? directory = null);
}
