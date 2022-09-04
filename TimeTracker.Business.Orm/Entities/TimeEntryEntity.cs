using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "time_entries")]
    public class TimeEntryEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "description", Length = 1000, NotNull = false)]
        public virtual string? Description { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "hourly_rate", NotNull = false)]
        public virtual decimal? HourlyRate { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "is_billable", NotNull = false)]
        public virtual bool IsBillable { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "start_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime StartTime { get; set; }
        
        [Property(NotNull = false, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "end_time", SqlType = "datetime", NotNull = false)]
        public virtual DateTime? EndTime { get; set; }
        
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
            Fetch = FetchMode.Join,
            Cascade = "none"
        )]
        public virtual WorkspaceEntity Workspace { get; set; }
        
        [ManyToOne(
            ClassType = typeof(ProjectEntity), 
            Column = "project_id", 
            Lazy = Laziness.Proxy,
            Fetch = FetchMode.Join,
            Cascade = "none"
        )]
        public virtual ProjectEntity? Project { get; set; }
        
        public virtual bool IsActive => EndTime == null;
        
        public virtual bool IsNew => Id == 0;
    }
}
