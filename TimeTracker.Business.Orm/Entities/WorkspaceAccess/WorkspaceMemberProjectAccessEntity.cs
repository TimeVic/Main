using NHibernate.Mapping.Attributes;
using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.WorkspaceAccess
{
    public class WorkspaceMemberProjectAccessEntity: AEntity
    {
        public virtual decimal? HourlyRate { get; set; }

        #region Relationships

        public virtual required WorkspaceMemberEntity WorkspaceMember { get; set; }
        public virtual required ProjectEntity Project { get; set; }

        #endregion
    }
}
