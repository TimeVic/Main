using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public interface IFileStorage: IDomainService
{
    Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        Stream fileStream,
        string fileName,
        StoredFileType fileType
    ) where TEntity : IEntity;

    Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        IFormFile formFile,
        StoredFileType fileType
    ) where TEntity : IEntity;
}
