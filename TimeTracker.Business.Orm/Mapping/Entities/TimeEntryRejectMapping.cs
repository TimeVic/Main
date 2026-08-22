using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class TimeEntryRejectMapping : BaseGuidMappings<TimeEntryRejectEntity>
{
    public TimeEntryRejectMapping()
    {
        Table("time_entry_rejects");

        Map(x => x.Reason);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();

        References(x => x.TimeEntry)
            .Column("time_entry_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();

        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
