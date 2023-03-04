using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public interface IFileStorage: IDomainService
{
    public Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        byte[] fileData,
        string fileName,
        StoredFileType fileType,
        CancellationToken cancellationToken = default
    ) where TEntity : IEntity;

    Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        IFormFile formFile,
        StoredFileType fileType,
        CancellationToken cancellationToken = default
    ) where TEntity : IEntity;

    Task<(StoredFileEntity, Stream)> GetFileStream(UserEntity user, long fileId);

    Task DeleteFile(UserEntity user, long fileId);

    Task<StoredFileEntity?> UploadFirstPendingToCloud(CancellationToken cancellationToken = default);
}
