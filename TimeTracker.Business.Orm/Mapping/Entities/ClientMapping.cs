using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class ClientMapping: BaseGuidMappings<ClientEntity>
{
    public ClientMapping()
    {
        Table("clients");
        
        Map(x => x.Name);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.Projects)
            .KeyColumn("client_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.ClientPayments)
            .KeyColumn("client_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
