using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Workspaces
{
    public class WorkspaceSettingsClickUpEntity: AEntity
    {   
        public virtual string? SecurityKey { get; set; }
        public virtual string? TeamId { get; set; }
        public virtual bool IsCustomTaskIds { get; set; } = true;
        public virtual bool IsFillTimeEntryWithTaskDetails { get; set; } = true;
        public virtual bool IsActive { get; set; }

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual required UserEntity User { get; set; }

        #endregion
    }
}
