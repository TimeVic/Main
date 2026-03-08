using Domain.Abstractions;
using FluentNHibernate.Mapping;

namespace TimeTracker.Business.Orm.Mapping.Common;

public class BaseGuidMappings<T>: ClassMap<T> where T : IEntity
{
    public BaseGuidMappings()
    {
        Id(x => x.Id)
            .Column("id")
            .GeneratedBy.Assigned()
            .Unique()
            .Not.Nullable()
            .CustomSqlType("uuid");
    }
}
