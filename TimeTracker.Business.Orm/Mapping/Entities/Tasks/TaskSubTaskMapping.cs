using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Tasks;

public class TaskSubTaskMapping : BaseGuidMappings<TaskSubTaskEntity>
{
    public TaskSubTaskMapping()
    {
        Table("task_sub_tasks");

        Map(x => x.Title);
        Map(x => x.IsCompleted);
        Map(x => x.PositionIndex);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();

        References(x => x.Task)
            .Column("task_id")
            .Fetch.Select()
            .LazyLoad();
    }
}
