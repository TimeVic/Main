using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class TimeEntryMapping: BaseGuidMappings<TimeEntryEntity>
{
    public TimeEntryMapping()
    {
        Table("time_entries");
        
        Map(x => x.Description).Nullable();
        Map(x => x.HourlyRate).DecimalNullable();
        Map(x => x.IsBillable);
        Map(x => x.StartTime).DateTime();
        Map(x => x.EndTime).DateTimeNullable();
        Map(x => x.TaskId).Nullable();
        Map(x => x.ClickUpId)
            .Column("clickup_id")
            .Nullable();
        Map(x => x.RedmineId).Nullable();
        Map(x => x.JiraId).Nullable();
        Map(x => x.IsMarkedToDelete);
        Map(x => x.IsAutostopped);
        Map(x => x.AutoStopWarningSentAt).DateTimeNullable();
        Map(x => x.TimeZone);
        Map(x => x.Status).CustomType<TimeEntryStatus>().CustomSqlType("smallint").Not.Nullable();
        
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.Project)
            .Column("project_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.User)
            .Column("user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        References(x => x.Task)
            .Column("internal_task_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasManyToMany(x => x.Tags)
            .Table("time_entry_tags")
            .ParentKeyColumn("time_entry_id")
            .ChildKeyColumn("tag_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();

        HasMany(x => x.Approvals)
            .KeyColumn("time_entry_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.AllDeleteOrphan()
            .Inverse();

        HasMany(x => x.Rejections)
            .KeyColumn("time_entry_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.AllDeleteOrphan()
            .Inverse();
    }
}
