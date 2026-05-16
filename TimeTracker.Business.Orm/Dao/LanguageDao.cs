using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class LanguageDao : BaseDao, ILanguageDao
{
    public const string DefaultLanguageCode = "en";

    public LanguageDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<LanguageEntity> GetDefaultAsync()
    {
        return await Session.Query<LanguageEntity>()
            .Where(item => item.Code == DefaultLanguageCode)
            .FirstAsync();
    }

    public async Task<LanguageEntity?> GetByCodeAsync(string code)
    {
        var normalizedCode = NormalizeCode(code);
        return await Session.Query<LanguageEntity>()
            .Where(item => item.Code == normalizedCode)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<LanguageEntity>> GetAllAsync()
    {
        return await Session.Query<LanguageEntity>()
            .OrderBy(item => item.Name)
            .ToListAsync();
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().Equals("en-En", StringComparison.OrdinalIgnoreCase)
            ? DefaultLanguageCode
            : code.Trim();
    }
}
