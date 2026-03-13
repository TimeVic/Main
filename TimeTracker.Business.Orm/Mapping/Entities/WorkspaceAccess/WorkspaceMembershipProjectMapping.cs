using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.WorkspaceAccess;

public class WorkspaceMembershipProjectMapping: BaseGuidMappings<WorkspaceMembershipProjectAccessEntity>
{
    public WorkspaceMembershipProjectMapping()
    {
        Table("workspace_membership_project_accesses");
        
        Map(x => x.HourlyRate).Nullable();
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.WorkspaceMembership)
            .Column("workspace_membership_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.Project)
            .Column("project_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
