using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.GoalsTracker;

public class GoalsTrackerItemMapping: BaseGuidMappings<GoalsTrackerItemEntity>
{
    public GoalsTrackerItemMapping()
    {
        Table("goals_tracker_items");
        
        Map(x => x.Name);
        Map(x => x.NumberOfTimes);
        Map(x => x.Position);
        Map(x => x.IsArchived);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Tracker)
            .Column("goals_tracker_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.CompletionMarkers)
            .KeyColumn("goals_tracker_item_id")
            .Fetch.Select()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
