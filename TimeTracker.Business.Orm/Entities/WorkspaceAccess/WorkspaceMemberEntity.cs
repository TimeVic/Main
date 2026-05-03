using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.WorkspaceAccess
{
    public class WorkspaceMemberEntity: AEntity
    {   
        public virtual MembershipAccessType Access { get; set; }

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual required UserEntity User { get; set; }
        public virtual ICollection<WorkspaceMemberProjectAccessEntity> ProjectAccesses { get; set; } = new List<WorkspaceMemberProjectAccessEntity>();

        #endregion
    }
}
