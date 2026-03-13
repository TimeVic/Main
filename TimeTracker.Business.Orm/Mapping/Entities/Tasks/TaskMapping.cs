using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Business.Orm.Mapping.Entities.Tasks;

public class TaskMapping: BaseGuidMappings<TaskEntity>
{
    public TaskMapping()
    {
        Table("tasks");
        
        Map(x => x.TaskId);
        Map(x => x.Status).Enum<TaskStatus>();
        Map(x => x.Priority).Enum<TaskPriority>();
        Map(x => x.Title);
        Map(x => x.Description);
        Map(x => x.StartTime).DateTimeNullable();
        Map(x => x.EndTime).DateTimeNullable();
        Map(x => x.RemindedTime).DateTimeNullable();
        Map(x => x.ReminderTime).DateTimeNullable();
        Map(x => x.IsArchived);
        Map(x => x.ExternalTaskId);
        Map(x => x.PositionIndex);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.TaskList)
            .Column("task_list_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasManyToMany(x => x.Tags)
            .Table("task_tags")
            .ParentKeyColumn("task_id")
            .ChildKeyColumn("tag_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
        
        HasManyToMany(x => x.Attachments)
            .Table("task_stored_files")
            .ParentKeyColumn("task_id")
            .ChildKeyColumn("stored_file_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
        
        HasMany(x => x.HistoryItems)
            .KeyColumn("task_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();
    }
}
