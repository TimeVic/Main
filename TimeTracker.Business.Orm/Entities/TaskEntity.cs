using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "tasks")]
    public class TaskEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "title", Length = 1024, NotNull = true)]
        public virtual string Title { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "description", Length = 10000, NotNull = false)]
        public virtual string? Description { get; set; }
        
        [Property(NotNull = false, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "notification_time", SqlType = "datetime", NotNull = false)]
        public virtual DateTime? NotificationTime { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "is_done", NotNull = true)]
        public virtual bool IsDone { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "is_archived", NotNull = true)]
        public virtual bool IsArchived { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "external_task_id", Length = 512, NotNull = false)]
        public virtual string? ExternalTaskId { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.Proxy,
            Fetch = FetchMode.Join,
            Cascade = "none"
        )]
        public virtual UserEntity User { get; set; }
        
        [ManyToOne(
            ClassType = typeof(TaskListEntity), 
            Column = "task_list_id", 
            Lazy = Laziness.Proxy,
            Fetch = FetchMode.Join,
            Cascade = "none"
        )]
        public virtual TaskListEntity TaskList { get; set; }
        
        [Set(
            Table = "task_stored_files",
            Lazy = CollectionLazy.True,
            Cascade = "none",
            BatchSize = 20
        )]
        [Key(
            Column = "task_id"
        )]
        [ManyToMany(
            Unique = true,
            ClassType = typeof(StoredFileEntity),
            Column = "stored_file_id"
        )]
        public virtual ICollection<StoredFileEntity> Attachments { get; set; } = new List<StoredFileEntity>();

        #region Calculated

        public virtual WorkspaceEntity Workspace => TaskList.Project.Workspace;

        #endregion
    }
}
