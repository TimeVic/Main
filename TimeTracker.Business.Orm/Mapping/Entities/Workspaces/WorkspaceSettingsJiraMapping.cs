using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Workspaces;

public class WorkspaceSettingsJiraMapping: BaseGuidMappings<WorkspaceSettingsJiraEntity>
{
    public WorkspaceSettingsJiraMapping()
    {
        Table("workspace_setting_jiras");
        
        Map(x => x.ApiKey);
        Map(x => x.UserName);
        Map(x => x.Url);
        Map(x => x.IsFillTimeEntryWithTaskDetails);
        Map(x => x.IsActive);
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
