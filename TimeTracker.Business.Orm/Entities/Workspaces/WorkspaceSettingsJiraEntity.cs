using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Workspaces
{
    public class WorkspaceSettingsJiraEntity: AEntity
    {
        public virtual string? ApiKey { get; set; }
        public virtual string? UserName { get; set; }
        public virtual string? Url { get; set; }
        public virtual bool IsFillTimeEntryWithTaskDetails { get; set; } = true;
        public virtual bool IsActive { get; set; }

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual required UserEntity User { get; set; }

        #endregion
    }
}
