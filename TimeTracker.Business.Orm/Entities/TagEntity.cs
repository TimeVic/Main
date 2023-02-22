using System.Drawing;
using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Hibernate.DataTypes;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "tags")]
    public class TagEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "name", Length = 200, NotNull = true)]
        public virtual string Name { get; set; }

        [Property(NotNull = false, TypeType = typeof(ColorType))]
        [Column(Name = "color", Length = 30, NotNull = false)]
        public virtual Color? Color { get; set; }
        
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
        
        [Set(
            Table = "task_tags",
            Lazy = CollectionLazy.True,
            Cascade = "none",
            BatchSize = 20
        )]
        [Key(
            Column = "tag_id"
        )]
        [ManyToMany(
            Unique = true,
            ClassType = typeof(TaskEntity),
            Column = "task_id"
        )]
        public virtual ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
        
        [Set(
            Table = "time_entry_tags",
            Lazy = CollectionLazy.True,
            Cascade = "none",
            BatchSize = 20
        )]
        [Key(
            Column = "tag_id"
        )]
        [ManyToMany(
            Unique = true,
            ClassType = typeof(TimeEntryEntity),
            Column = "time_entry_id"
        )]
        public virtual ICollection<TimeEntryEntity> TimeEntries { get; set; } = new List<TimeEntryEntity>();
    }
}
