using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Business.Orm.Entities.Tasks
{
    public class TaskHistoryItemEntity: AEntity
    {   
        public virtual TaskStatus Status { get; set; }
        public virtual TaskPriority Priority { get; set; }
        public virtual required string Title { get; set; }
        public virtual string? Description { get; set; }
        public virtual string? Tags { get; set; }
        public virtual string? Attachments { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual string? ExternalTaskId { get; set; }
        public virtual bool IsNotified { get; set; }
        public virtual bool IsNewTask { get; set; }

        #region Relationships

        public virtual required TaskEntity Task { get; set; }
        public virtual required UserEntity User { get; set; }
        public virtual required UserEntity AssigneeUser { get; set; }
        public virtual required TaskListEntity TaskList { get; set; }

        #endregion

        #region Calculated

        public virtual WorkspaceEntity Workspace => TaskList.Project.Client.Workspace;

        #endregion
    }
}
