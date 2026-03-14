using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Messaging;

public class MessagingActivityMapping: BaseGuidMappings<MessagingActivityEntity>
{
    public MessagingActivityMapping()
    {
        Table("counters");
        Schema("messaging");
        
        Map(x => x.IsWriting);
        Map(x => x.WritingStartedAt).DateTimeNullable();
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Channel)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.User)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
