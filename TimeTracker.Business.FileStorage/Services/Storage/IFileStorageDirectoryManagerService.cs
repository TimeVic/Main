using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Business.FileStorage.Services.Storage;

public interface IFileStorageDirectoryManagerService: IDomainService
{
    Task<FileStorageDirectoryEntity?> CreateRecursive(FileStorageBucketEntity bucket, string? path);

    ICollection<FileStorageDirectoryEntity> GetTreeBranchByPath(FileStorageBucketEntity bucket, string? path);

    ICollection<FileStorageDirectoryEntity> GetTreeBranchAsListByPath(FileStorageBucketEntity bucket, string? path);
}
