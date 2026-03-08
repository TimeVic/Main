using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Workspaces;

public class WorkspaceMapping: BaseGuidMappings<WorkspaceEntity>
{
    public WorkspaceMapping()
    {
        Table("workspaces");
        
        Map(x => x.Name);
        Map(x => x.IsDefault);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.CreatedUser)
            .Column("created_user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.Clients)
            .KeyColumn("workspace_id")
            .Fetch.Join()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.Projects)
            .KeyColumn("workspace_id")
            .Fetch.Join()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.TimeEntries)
            .KeyColumn("workspace_id")
            .Fetch.Join()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.SettingsClickUp)
            .KeyColumn("workspace_id")
            .Fetch.Join()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.SettingsRedmine)
            .KeyColumn("workspace_id")
            .Fetch.Join()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.SettingsJira)
            .KeyColumn("workspace_id")
            .Fetch.Join()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.Memberships)
            .KeyColumn("workspace_id")
            .Fetch.Join()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.Tags)
            .KeyColumn("workspace_id")
            .Fetch.Join()
            .ExtraLazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
