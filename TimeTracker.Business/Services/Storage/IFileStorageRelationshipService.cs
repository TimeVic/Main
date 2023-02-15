using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public interface IFileStorageRelationshipService: IDomainService
{
    Task<IEntity> GetFileRelationship(
        long entityId,
        StorageEntityType entityType
    );

    Task AddFileRelationship<TEntity>(
        TEntity entity,
        StoredFileEntity file
    ) where TEntity : IEntity;
}
