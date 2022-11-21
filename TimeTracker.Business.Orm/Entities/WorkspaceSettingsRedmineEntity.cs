using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "workspace_setting_redmines")]
    public class WorkspaceSettingsRedmineEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "url", Length = 200, NotNull = true)]
        public virtual string Url { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "api_key", Length = 100, NotNull = true)]
        public virtual string ApiKey { get; set; }

        [Property(NotNull = true)]
        [Column(Name = "redmine_user_id", NotNull = true)]
        public virtual long RedmineUserId { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "activity_id", NotNull = true)]
        public virtual long ActivityId { get; set; }
        
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
