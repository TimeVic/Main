using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.GoalsTracker;

public class GoalsTrackerCompletionMarkerMapping: BaseGuidMappings<GoalsTrackerCompletionMarkerEntity>
{
    public GoalsTrackerCompletionMarkerMapping()
    {
        Table("goals_tracker_completion_markers");
        
        Map(x => x.DayOfMonth);
        Map(x => x.IsChecked);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.GoalsTrackerItem)
            .Column("goals_tracker_item_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
