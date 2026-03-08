using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Notifications;

public class NotificationMapping: BaseGuidMappings<NotificationEntity>
{
    public NotificationMapping()
    {
        Table("notifications");
        
        Map(x => x.Type).Enum<NotificationActionType>();
        Map(x => x.IsRead);
        Map(x => x.Comment);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.PerformedUser)
            .Column("performed_user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.ReceiverUser)
            .Column("receiver_user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
