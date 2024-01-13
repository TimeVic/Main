using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.GoalsTracker
{
    [Class(Table = "goals_tracker_completion_markers")]
    public class GoalsTrackerCompletionMarkerEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "day_of_month", NotNull = true)]
        public virtual int DayOfMonth { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "is_checked", NotNull = true)]
        public virtual bool IsChecked { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(GoalsTrackerItemEntity), 
            Column = "goals_tracker_item_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual GoalsTrackerItemEntity GoalsTrackerItem { get; set; }
    }
}
