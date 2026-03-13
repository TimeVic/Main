using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Workspaces
{
    public class WorkspaceSettingsRedmineEntity: AEntity
    {   
        public virtual required string Url { get; set; }
        public virtual required string ApiKey { get; set; }
        public virtual long RedmineUserId { get; set; }
        public virtual long ActivityId { get; set; }
        public virtual bool IsActive { get; set; }

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual required UserEntity User { get; set; }

        #endregion
    }
}
