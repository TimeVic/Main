using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Workspaces;

public class WorkspaceSettingsClickUpMapping: BaseGuidMappings<WorkspaceSettingsClickUpEntity>
{
    public WorkspaceSettingsClickUpMapping()
    {
        Table("workspace_setting_clickups");
        
        Map(x => x.SecurityKey);
        Map(x => x.IsCustomTaskIds);
        Map(x => x.IsFillTimeEntryWithTaskDetails);
        Map(x => x.IsActive);
        Map(x => x.TeamId);
        
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
    }
}
