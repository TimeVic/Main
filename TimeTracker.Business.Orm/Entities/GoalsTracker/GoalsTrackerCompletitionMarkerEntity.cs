using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.GoalsTracker
{
    public class GoalsTrackerCompletionMarkerEntity: AEntity
    {
        public virtual int DayOfMonth { get; set; }
        public virtual bool IsChecked { get; set; }
        
        [ManyToOne(
            ClassType = typeof(GoalsTrackerItemEntity), 
            Column = "goals_tracker_item_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual required GoalsTrackerItemEntity GoalsTrackerItem { get; set; }
    }
}
