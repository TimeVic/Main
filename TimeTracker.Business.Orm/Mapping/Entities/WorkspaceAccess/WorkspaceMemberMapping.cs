using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.WorkspaceAccess;

public class WorkspaceMemberMapping: BaseGuidMappings<WorkspaceMemberEntity>
{
    public WorkspaceMemberMapping()
    {
        Table("workspace_members");
        
        Map(x => x.Access)
            .Column("membership_access_type_id")
            .Enum<MembershipAccessType>();
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.ProjectAccesses)
            .KeyColumn("workspace_member_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.AllDeleteOrphan()
            .Inverse();
    }
}
