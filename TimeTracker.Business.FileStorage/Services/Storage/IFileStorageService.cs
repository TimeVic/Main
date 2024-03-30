using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Business.FileStorage.Services.Storage;

public interface IFileStorageService: IDomainService
{
    Task<FileStorageFileEntity> Put(FileStorageBucketEntity bucket, IFormFile file);
    
    Task<FileStorageFileEntity> Put(FileStorageFileEntity file, Stream fileStream);
}
