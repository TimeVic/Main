using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities
{
    public class PaymentEntity: AEntity
    {   
        public virtual DateTime PaymentTime { get; set; }
        public virtual string? Description { get; set; }
        public virtual decimal Amount { get; set; }

        #region Relationships

        public virtual ProjectEntity? Project { get; set; }
        public virtual required ClientEntity Client { get; set; }
        public virtual required UserEntity User { get; set; }
        public virtual required WorkspaceEntity Workspace { get; set; }

        #endregion
    }
}
