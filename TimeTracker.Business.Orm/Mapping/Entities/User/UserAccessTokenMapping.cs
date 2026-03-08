using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.User;

public class UserAccessTokenMapping: BaseGuidMappings<UserAccessTokenEntity>
{
    public UserAccessTokenMapping()
    {
        Table("user_access_tokens");
        
        Map(x => x.Token);
        Map(x => x.ExpirationTime).DateTime();
        Map(x => x.CreatedAt).DateTime();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.JwtTokens)
            .KeyColumn("access_token_id")
            .Fetch.Select()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
