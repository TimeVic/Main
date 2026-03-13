using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.FileStorage;

public class FileStorageAccessKeyMapping: BaseGuidMappings<FileStorageAccessKeyEntity>
{
    public FileStorageAccessKeyMapping()
    {
        Table("fs_access_keys");
        
        Map(x => x.AccessKey);
        Map(x => x.SecretKey);
        Map(x => x.ExpirationTime).DateTimeNullable();
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
