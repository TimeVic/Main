using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface ILanguageDao : IDomainService
{
    Task<LanguageEntity> GetDefaultAsync();

    Task<LanguageEntity?> GetByCodeAsync(string code);

    Task<ICollection<LanguageEntity>> GetAllAsync();
}
