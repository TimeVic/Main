using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Tasks;

public class TaskCommentMapping: BaseGuidMappings<TaskCommentEntity>
{
    public TaskCommentMapping()
    {
        Table("task_comments");
        
        Map(x => x.Comment);
        Map(x => x.IsArchived);
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
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
        
        HasManyToMany(x => x.Watchers)
            .Table("task_comment_watchers")
            .ParentKeyColumn("comment_id")
            .ChildKeyColumn("user_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
        
        HasManyToMany(x => x.Attachments)
            .Table("task_comment_stored_files")
            .ParentKeyColumn("comment_id")
            .ChildKeyColumn("stored_file_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
    }
}
