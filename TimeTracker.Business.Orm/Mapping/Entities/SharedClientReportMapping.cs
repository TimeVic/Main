using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class SharedClientReportMapping : BaseGuidMappings<SharedClientReportEntity>
{
    public SharedClientReportMapping()
    {
        Table("shared_client_reports");

        Map(x => x.Token).Length(64).Unique();
        Map(x => x.IsActive);
        Map(x => x.IsShowTasks);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();

        References(x => x.Client)
            .Column("client_id")
            .Unique()
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
