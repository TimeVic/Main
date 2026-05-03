using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class ClientPaymentMapping: BaseGuidMappings<ClientPaymentEntity>
{
    public ClientPaymentMapping()
    {
        Table("client_payments");

        Map(x => x.PaymentTime).DateTime();
        Map(x => x.Description);
        Map(x => x.Amount);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();

        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();

        References(x => x.Client)
            .Column("client_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();

        References(x => x.Project)
            .Column("project_id")
            .Fetch.Select()
            .LazyLoad()
            .Nullable()
            .Cascade.SaveUpdate();
    }
}
