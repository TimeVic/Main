using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface ICurrencyDao: IDomainService
{
    Task<CurrencyEntity> GetDefault();

    Task<CurrencyEntity?> GetBy(Guid id);

    Task<ICollection<CurrencyEntity>> GetAll();
}
