using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities
{
    public class ProjectEntity: AEntity
    {
        public virtual required string Name { get; set; }
        public virtual bool IsBillableByDefault { get; set; } = true;
        public virtual decimal? DefaultHourlyRate { get; set; }
        public virtual bool IsArchived { get; set; } = false;

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual ClientEntity? Client { get; set; }

        public virtual ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
        public virtual ICollection<WorkspaceMembershipProjectAccessEntity> MembershipProjectAccess { get; set; } = new List<WorkspaceMembershipProjectAccessEntity>();
        public virtual ICollection<TaskListEntity> TaskLists { get; set; } = new List<TaskListEntity>();

        #endregion
        
        public virtual void SetClient(ClientEntity? client)
        {
            if (Client?.Id == client?.Id)
            {
                return;
            }

            Client = client;
            client?.Projects.Add(this);
        }
        
        public virtual void AddPayment(PaymentEntity payment)
        {
            Payments.Add(payment);
            payment.Project = this;
        }
    }
}
