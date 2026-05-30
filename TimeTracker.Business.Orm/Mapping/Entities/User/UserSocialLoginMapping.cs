using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.User;

public class UserSocialLoginMapping: BaseGuidMappings<UserSocialLoginEntity>
{
    public UserSocialLoginMapping()
    {
        Table("user_social_logins");

        Map(x => x.GoogleId).Nullable();
        Map(x => x.GoogleAccessToken).Nullable();
        Map(x => x.GoogleRefreshToken).Nullable();
        Map(x => x.GoogleConnectedAt).DateTimeNullable();

        Map(x => x.FacebookId).Nullable();
        Map(x => x.FacebookAccessToken).Nullable();
        Map(x => x.FacebookRefreshToken).Nullable();
        Map(x => x.FacebookConnectedAt).DateTimeNullable();

        Map(x => x.AppleId).Nullable();
        Map(x => x.AppleAccessToken).Nullable();
        Map(x => x.AppleRefreshToken).Nullable();
        Map(x => x.AppleConnectedAt).DateTimeNullable();

        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        Map(x => x.DeletedAt).DateTimeNullable();

        References(x => x.User)
            .Column("user_id")
            .Unique()
            .Fetch.Select()
            .LazyLoad()
            .Cascade.None();
    }
}
