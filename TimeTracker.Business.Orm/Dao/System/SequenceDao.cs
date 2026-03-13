using Domain.Abstractions;
using NHibernate;
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
        var result = await _dbSessionProvider.CurrentSession.CreateSQLQuery(@"SELECT * FROM fn_sequence_get_next(:entity_name, :entity_id)")
            .SetParameter("entity_name", entity.GetType().Name)
            .SetParameter("entity_id", entity.Id)
            .SetFlushMode(FlushMode.Always)
            .ListAsync<long>();
        if (result == null)
            throw new Exception($"Sequence value was not generated: {entity.GetType().Name} with id {entity.Id}");
        return result.First();
    }
}
