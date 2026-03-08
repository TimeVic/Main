using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.User;

public class UserMapping: BaseGuidMappings<UserEntity>
{
    public UserMapping()
    {
        Table("users");
        
        Map(x => x.UserName);
        Map(x => x.Email);
        Map(x => x.Timezone);
        Map(x => x.VerificationToken);
        Map(x => x.VerificationTime).DateTimeNullable();
        Map(x => x.PasswordSalt);
        Map(x => x.PasswordHash);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        HasMany(x => x.CreatedWorkspaces)
            .KeyColumn("created_user_id")
            .Fetch.Select()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.TimeEntries)
            .KeyColumn("user_id")
            .Fetch.Select()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.WorkspaceMemberships)
            .KeyColumn("user_id")
            .Fetch.Select()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.NotificationTokens)
            .KeyColumn("user_id")
            .Fetch.Select()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
