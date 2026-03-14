using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Messaging;

public class MessagingChannelMemberMapping: BaseGuidMappings<MessagingChannelMemberEntity>
{
    public MessagingChannelMemberMapping()
    {
        Table("channel_members");
        Schema("messaging");
        
        Map(x => x.DeactivatedAt).DateTimeNullable();
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Channel)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.Member)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
