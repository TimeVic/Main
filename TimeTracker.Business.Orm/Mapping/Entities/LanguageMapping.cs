using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class LanguageMapping : BaseGuidMappings<LanguageEntity>
{
    public LanguageMapping()
    {
        Table("languages");

        Map(x => x.Name);
        Map(x => x.Code);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
    }
}
