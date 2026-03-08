using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class PaymentMapping: BaseGuidMappings<PaymentEntity>
{
    public PaymentMapping()
    {
        Table("payments");
        
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
            .Nullable()
            .Cascade.SaveUpdate();
        
        References(x => x.Project)
            .Column("project_id")
            .Fetch.Select()
            .LazyLoad()
            .Nullable()
            .Cascade.SaveUpdate();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Nullable()
            .Cascade.SaveUpdate();
    }
}
