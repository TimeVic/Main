using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities;

public class ClientPaymentEntity: AEntity
{
    public virtual DateTime PaymentTime { get; set; }
    public virtual string? Description { get; set; }
    public virtual decimal Amount { get; set; }

    #region Relationships

    public virtual WorkspaceEntity Workspace => Client.Workspace;

    public virtual required ClientEntity Client { get; set; }
    public virtual ProjectEntity? Project { get; set; }

    #endregion
}
