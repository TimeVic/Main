using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.User;

public class UserNotificationTokenMapping: BaseGuidMappings<UserNotificationTokenEntity>
{
    public UserNotificationTokenMapping()
    {
        Table("user_notification_tokens");
        
        Map(x => x.Token);
        Map(x => x.CreatedAt).DateTime();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
