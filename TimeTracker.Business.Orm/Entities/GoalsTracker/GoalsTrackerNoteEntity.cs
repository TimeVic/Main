using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities.GoalsTracker
{
    [Class(Table = "goals_tracker_notes")]
    public class GoalsTrackerNoteEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }

        [Property(NotNull = true)]
        [Column(Name = "text", Length = 5064, NotNull = true)]
        public virtual string Text { get; set; } = string.Empty;
        
        [Property(NotNull = true)]
        [Column(Name = "is_archived", NotNull = true)]
        public virtual bool IsArchived { get; set; } = false;
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(GoalsTrackerEntity), 
            Column = "goals_tracker_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual GoalsTrackerEntity Tracker { get; set; }
    }
}
