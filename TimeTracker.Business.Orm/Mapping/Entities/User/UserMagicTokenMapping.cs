using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.User;

public class UserMagicTokenMapping : BaseGuidMappings<UserMagicTokenEntity>
{
    public UserMagicTokenMapping()
    {
        Table("user_magic_tokens");

        Map(x => x.Token);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.ExpirationTime).DateTime();

        References(x => x.User)
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
