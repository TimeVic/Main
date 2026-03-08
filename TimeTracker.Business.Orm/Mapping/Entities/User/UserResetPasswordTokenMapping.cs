using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.User;

public class UserResetPasswordTokenMapping: BaseGuidMappings<UserResetPasswordRequestEntity>
{
    public UserResetPasswordTokenMapping()
    {
        Table("user_reset_password_requests");
        
        Map(x => x.VerificationToken);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.ExpirationTime).DateTime();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
