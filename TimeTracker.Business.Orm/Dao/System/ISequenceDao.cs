using Domain.Abstractions;

namespace TimeTracker.Business.Orm.Dao.System;

public interface ISequenceDao: IDomainService
{
    Task<long> GetNextValue<TEntity>(TEntity entity) where TEntity: IEntity;
}
