using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.GoalsTracker;

public class GoalsTrackerMapping: BaseGuidMappings<GoalsTrackerEntity>
{
    public GoalsTrackerMapping()
    {
        Table("goals_trackers");
        
        Map(x => x.Year);
        Map(x => x.Month);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.Items)
            .KeyColumn("goals_tracker_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.Notes)
            .KeyColumn("goals_tracker_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
