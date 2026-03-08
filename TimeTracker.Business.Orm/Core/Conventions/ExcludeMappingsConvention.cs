using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Core.Conventions;

public class ExcludeMappingsConvention : IClassConvention
{
    public static readonly HashSet<Type> ExcludedMappings =
    [
        typeof(BaseGuidMappings<>)
    ];

    public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
    {
        criteria.Expect(x => !ExcludedMappings.Contains(x.EntityType));
    }

    public void Apply(IClassInstance instance)
    {
    }
}
