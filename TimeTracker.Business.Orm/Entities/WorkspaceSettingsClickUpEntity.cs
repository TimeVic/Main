using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "workspace_setting_clickups")]
    public class WorkspaceSettingsClickUpEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "security_key", Length = 512, NotNull = false)]
        public virtual string? SecurityKey { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "team_id", Length = 512, NotNull = false)]
        public virtual string? TeamId { get; set; }

        [Property(NotNull = true)]
        [Column(Name = "is_custom_task_ids", NotNull = true)]
        public virtual Boolean IsCustomTaskIds { get; set; } = true;

        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(WorkspaceEntity), 
            Lazy = Laziness.False,
            Fetch = FetchMode.Join,
            Column = "workspace_id", 
            Cascade = "none",
            Unique = true
        )]
        public virtual WorkspaceEntity Workspace { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Lazy = Laziness.False,
            Fetch = FetchMode.Join,
            Column = "user_id", 
            Cascade = "none",
            Unique = true
        )]
        public virtual UserEntity User { get; set; }
    }
}
