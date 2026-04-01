using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class CurrencyDao: BaseDao, ICurrencyDao
{
    public CurrencyDao(ILifetimeScope scope): base(scope)
    {
    }

    public Task<CurrencyEntity> GetDefault()
    {
        return Session.Query<CurrencyEntity>()
            .Where(item => item.Code == GlobalConstants.DefaultCurrencyCode)
            .FirstAsync();
    }
    
    public async Task<CurrencyEntity?> GetBy(Guid id)
    {
        return await Session.Query<CurrencyEntity>()
            .FirstOrDefaultAsync(item => item.Id == id);
    }
    
    public async Task<ICollection<CurrencyEntity>> GetAll()
    {
        return await Session.Query<CurrencyEntity>()
            .ToListAsync();
    }
}
