using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.GoalsTracker;

public class GoalsTrackerNoteMapping: BaseGuidMappings<GoalsTrackerNoteEntity>
{
    public GoalsTrackerNoteMapping()
    {
        Table("goals_trackers");
        
        Map(x => x.Text);
        Map(x => x.IsArchived);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Tracker)
            .Column("goals_tracker_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
