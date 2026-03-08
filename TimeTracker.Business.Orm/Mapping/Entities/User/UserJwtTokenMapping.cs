using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.User;

public class UserJwtTokenMapping: BaseGuidMappings<UserJwtTokenEntity>
{
    public UserJwtTokenMapping()
    {
        Table("user_jwt_tokens");
        
        Map(x => x.Token);
        Map(x => x.ExpirationTime).DateTime();
        Map(x => x.CreatedAt).DateTime();
        
        References(x => x.AccessToken)
            .Column("access_token_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
