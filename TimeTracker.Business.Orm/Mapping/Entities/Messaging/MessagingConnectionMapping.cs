using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Messaging;

public class MessagingConnectionMapping: BaseGuidMappings<MessagingConnectionEntity>
{
    public MessagingConnectionMapping()
    {
        Table("connections");
        Schema("messaging");
        
        Map(x => x.ConnectionId);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.User)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
