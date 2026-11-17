using Domain.Abstractions;
using NHibernate;
using NHibernate.Proxy;
using Persistence.Transactions.Behaviors;

namespace TimeTracker.Business.Orm.Dao.System;

public class SequenceDao: ISequenceDao
{
    private readonly IDbSessionProvider _dbSessionProvider;

    public SequenceDao(IDbSessionProvider dbSessionProvider)
    {
        _dbSessionProvider = dbSessionProvider;
    }
    
    public async Task<long> GetNextValue<TEntity>(TEntity entity) where TEntity: IEntity
    {
        var entityType = NHibernateProxyHelper.GetClassWithoutInitializingProxy(entity);
        return await GetNextValue(entityType.Name, entity.Id);
    }

    public async Task<long> GetNextValue(string sequenceName, Guid entityId)
    {
        var result = await _dbSessionProvider.CurrentSession.CreateSQLQuery(@"SELECT * FROM fn_sequence_get_next(:entity_name, :entity_id)")
            .SetParameter("entity_name", sequenceName)
            .SetParameter("entity_id", entityId)
            .SetFlushMode(FlushMode.Always)
            .ListAsync<long>();
        if (result == null)
            throw new Exception($"Sequence value was not generated: {sequenceName} with id {entityId}");
        return result.First();
    }
}
