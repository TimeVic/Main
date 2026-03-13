using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class ProjectMapping: BaseGuidMappings<ProjectEntity>
{
    public ProjectMapping()
    {
        Table("projects");
        
        Map(x => x.Name);
        Map(x => x.IsBillableByDefault);
        Map(x => x.DefaultHourlyRate);
        Map(x => x.IsArchived);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.Client)
            .Column("client_id")
            .Fetch.Select()
            .LazyLoad()
            .Nullable()
            .Cascade.SaveUpdate();
        
        HasMany(x => x.Payments)
            .KeyColumn("project_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.MembershipProjectAccess)
            .KeyColumn("project_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
        
        HasMany(x => x.TaskLists)
            .KeyColumn("project_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
