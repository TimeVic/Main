using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities;

public class LanguageEntity : AEntity
{
    public virtual required string Name { get; set; }

    public virtual required string Code { get; set; }
}
