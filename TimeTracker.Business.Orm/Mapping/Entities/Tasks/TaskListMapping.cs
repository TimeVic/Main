using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Tasks;

public class TaskListMapping: BaseGuidMappings<TaskListEntity>
{
    public TaskListMapping()
    {
        Table("task_lists");
        
        Map(x => x.Name);
        Map(x => x.IsArchived);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Project)
            .Column("project_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
