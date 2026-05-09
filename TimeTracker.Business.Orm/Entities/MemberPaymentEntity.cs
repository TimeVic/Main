using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Orm.Entities
{
    public class MemberPaymentEntity: AEntity
    {   
        public virtual DateTime PaymentTime { get; set; }
        public virtual string? Description { get; set; }
        public virtual decimal Amount { get; set; }

        #region Relationships

        public virtual required ProjectEntity Project { get; set; }
        public virtual required WorkspaceMemberEntity Member { get; set; }

        #endregion
    }
}
