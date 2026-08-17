using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities
{
    public class 
        TimeEntryEntity : AEntity
    {
        public virtual string? Description { get; set; }
        public virtual decimal? HourlyRate { get; set; }
        public virtual bool IsBillable { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public virtual string? TaskId { get; set; }
        public virtual string? ClickUpId { get; set; }
        public virtual string? RedmineId { get; set; }
        public virtual long? JiraId { get; set; }
        public virtual bool IsMarkedToDelete { get; set; }
        public virtual bool IsAutostopped { get; set; }
        public virtual DateTime? AutoStopWarningSentAt { get; set; }
        public virtual string TimeZone { get; set; } = "UTC";
        
        #region Relationships
        public virtual WorkspaceEntity Workspace { get; set; } = null!;
        
        public virtual ProjectEntity? Project { get; set; }

        public virtual UserEntity User { get; set; } = null!;

        public virtual TaskEntity? Task { get; set; }
        
        public virtual ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
        #endregion
        
        #region Calculated

        public virtual bool IsSynced => !string.IsNullOrEmpty(RedmineId) 
            || !string.IsNullOrEmpty(ClickUpId);

        public virtual bool IsActive => EndTime == null;

        public virtual TimeSpan Duration => EndTime != null ? EndTime.Value - StartTime : TimeSpan.Zero;

        public virtual string? ExternalTaskId => Task?.ExternalTaskId;

        #endregion
    }
}
