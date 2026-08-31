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
        Map(x => x.Login);
        Map(x => x.Timezone);
        Map(x => x.VerificationToken);
        Map(x => x.VerificationTime).DateTimeNullable();
        Map(x => x.PasswordSalt);
        Map(x => x.PasswordHash);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();

        References(x => x.SelectedWorkspace)
            .Column("selected_workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Nullable()
            .Cascade.None();

        References(x => x.Language)
            .Column("language_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.None();
        
        HasMany(x => x.CreatedWorkspaces)
            .KeyColumn("created_user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.TimeEntries)
            .KeyColumn("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.WorkspaceMembers)
            .KeyColumn("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.NotificationTokens)
            .KeyColumn("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();

        HasOne(x => x.SocialLoginInfo)
            .PropertyRef("User")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.MessageCounters)
            .KeyColumn("user_id")
            .LazyLoad()
            .Inverse()
            .Fetch.Select()
            .Cascade.SaveUpdate()
            .AsSet();
        
        HasManyToMany(x => x.Avatars)
            .Table("user_stored_files")
            .ParentKeyColumn("user_id")
            .ChildKeyColumn("stored_file_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
    }
}
