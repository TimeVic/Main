using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.GoalsTracker
{
    [Class(Table = "goals_trackers")]
    public class GoalsTrackerEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "year", NotNull = true)]
        public virtual int Year { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "month", NotNull = true)]
        public virtual int Month { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(WorkspaceEntity), 
            Column = "workspace_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual WorkspaceEntity Workspace { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual UserEntity User { get; set; }
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "none"
        )]
        [Key(Column = "goals_tracker_id")]
        [OneToMany(ClassType = typeof(GoalsTrackerItemEntity))]
        public virtual ICollection<GoalsTrackerItemEntity> Items { get; set; } = new List<GoalsTrackerItemEntity>();
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "none"
        )]
        [Key(Column = "goals_tracker_id")]
        [OneToMany(ClassType = typeof(GoalsTrackerNoteEntity))]
        public virtual ICollection<GoalsTrackerNoteEntity> Notes { get; set; } = new List<GoalsTrackerNoteEntity>();
        
        #region Calculated

        public virtual IEnumerable<GoalsTrackerItemEntity> ActiveItems => Items.Where(item => !item.IsArchived);

        public virtual int DaysInCurrentMonth => DateTime.DaysInMonth(Year, Month);
        
        #endregion
    }
}
