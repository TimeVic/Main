using NHibernate.Mapping.Attributes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.Notifications
{
    public class NotificationEntity: AEntity
    {
        public virtual NotificationActionType Type { get; set; }
        public virtual bool IsRead { get; set; } = false;
        public virtual string? Comment { get; set; }

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual required UserEntity PerformedUser { get; set; }
        public virtual required UserEntity ReceiverUser { get; set; }

        #endregion
        
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
