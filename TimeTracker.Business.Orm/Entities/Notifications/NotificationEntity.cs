using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.Notifications
{
    [Class(Table = "notifications")]
    public class NotificationEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "type", SqlType = "int", NotNull = true)]
        public virtual NotificationActionType Type { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "is_read", NotNull = true)]
        public virtual bool IsRead { get; set; } = false;
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; } = DateTime.UtcNow;
        
        [Property(NotNull = false)]
        [Column(Name = "comment", Length = 2056, NotNull = false)]
        public virtual string? Comment { get; set; }

        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; } = DateTime.UtcNow;
        
        [ManyToOne(
            ClassType = typeof(WorkspaceEntity), 
            Column = "workspace_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual WorkspaceEntity Workspace { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "performed_user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual UserEntity PerformedUser { get; set; }

        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "receiver_user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual UserEntity ReceiverUser { get; set; }
        
        #region Optional
        
        [ManyToOne(
            ClassType = typeof(TaskEntity), 
            Column = "task_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual TaskEntity? Task { get; set; }
        
        [ManyToOne(
            ClassType = typeof(TaskCommentEntity), 
            Column = "task_comment_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual TaskCommentEntity? TaskComment { get; set; }
        
        #endregion
    }
}
