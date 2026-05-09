using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities
{
    public class ClientEntity: AEntity
    {
        public virtual required string Name { get; set; }

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
        public virtual ICollection<ClientPaymentEntity> ClientPayments { get; set; } = new List<ClientPaymentEntity>();

        #endregion

        public virtual void AddClientPayment(ClientPaymentEntity payment)
        {
            ClientPayments.Add(payment);
            payment.Client = this;
        }
    }
}
