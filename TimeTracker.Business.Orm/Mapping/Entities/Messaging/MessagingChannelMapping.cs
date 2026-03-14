using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Messaging;

public class MessagingChannelMapping: BaseGuidMappings<MessagingChannelEntity>
{
    public MessagingChannelMapping()
    {
        Table("channels");
        Schema("messaging");
        
        Map(x => x.Type).Enum<MessagingChannelType>();
        Map(x => x.Name);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Workspace)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.CreatedBy)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.Messages)
            .KeyColumn("channel_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.Members)
            .KeyColumn("channel_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse()
            .AsSet();
    }
}
