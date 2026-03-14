using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Messaging;

public class MessagingMessageMapping: BaseGuidMappings<MessagingMessageEntity>
{
    public MessagingMessageMapping()
    {
        Table("messages");
        Schema("messaging");
        
        Map(x => x.Text);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Channel)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.CreatedBy)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
