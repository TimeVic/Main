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
    public class TaskEntity: AEntity
    {
        public virtual long TaskId { get; set; }
        public virtual TaskStatus Status { get; set; }
        public virtual TaskPriority Priority { get; set; }
        public virtual required string Title { get; set; }
        public virtual string? Description { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual string? ExternalTaskId { get; set; }
        public virtual int PositionIndex { get; set; }
        
        #region Reminder
        public virtual DateTime? ReminderTime { get; set; }
        public virtual DateTime? RemindedTime { get; set; }
        
        #endregion

        #region Relationships

        public virtual required UserEntity User { get; set; }
        public virtual required TaskListEntity TaskList { get; set; }
        public virtual ICollection<StoredFileEntity> Attachments { get; set; } = new List<StoredFileEntity>();
        public virtual ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
        public virtual ICollection<TaskHistoryItemEntity> HistoryItems { get; set; } = new List<TaskHistoryItemEntity>();

        #endregion
        
        #region Calculated

        public virtual WorkspaceEntity Workspace => TaskList.Project.Workspace;

        public virtual string TagsString => string.Join(";", Tags.Select(item => item.Name));
        
        public virtual string AttachmentsString => string.Join(";", Tags.Select(item => item.Name));
        
        #endregion
    }
}
