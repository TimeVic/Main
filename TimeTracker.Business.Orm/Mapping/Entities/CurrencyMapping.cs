using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class CurrencyMapping: BaseGuidMappings<CurrencyEntity>
{
    public CurrencyMapping()
    {
        Table("currencies");
        
        Map(x => x.Code);
        Map(x => x.Symbol);
    }
}
