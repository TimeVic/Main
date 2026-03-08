using NHibernate.Mapping.Attributes;
using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.WorkspaceAccess
{
    public class WorkspaceMembershipProjectAccessEntity: AEntity
    {
        public virtual decimal? HourlyRate { get; set; }

        #region Relationships

        public virtual required WorkspaceMembershipEntity WorkspaceMembership { get; set; }
        public virtual required ProjectEntity Project { get; set; }

        #endregion
    }
}
