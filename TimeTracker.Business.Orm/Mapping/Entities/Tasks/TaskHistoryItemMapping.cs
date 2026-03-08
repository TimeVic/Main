using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace TimeTracker.Business.Orm.Mapping.Entities.Tasks;

public class TaskHistoryItemMapping: BaseGuidMappings<TaskHistoryItemEntity>
{
    public TaskHistoryItemMapping()
    {
        Table("task_history_items");
        
        Map(x => x.Status).Enum<TaskStatus>();
        Map(x => x.Priority).Enum<TaskPriority>();
        Map(x => x.Title);
        Map(x => x.Description);
        Map(x => x.Tags);
        Map(x => x.Attachments);
        Map(x => x.StartTime).DateTimeNullable();
        Map(x => x.EndTime).DateTimeNullable();
        Map(x => x.IsArchived);
        Map(x => x.ExternalTaskId);
        Map(x => x.IsNotified);
        Map(x => x.IsNewTask);
        Map(x => x.CreatedAt).DateTime();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.Task)
            .Column("task_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.AssigneeUser)
            .Column("assignee_user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.TaskList)
            .Column("task_list_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
