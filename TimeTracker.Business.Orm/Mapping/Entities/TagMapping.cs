using System.Drawing;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Hibernate.DataTypes;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class TagMapping: BaseGuidMappings<TagEntity>
{
    public TagMapping()
    {
        Table("tags");
        
        Map(x => x.Name);
        Map(x => x.Color)
            .CustomType<ColorType>()
            .Nullable();
        
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();
        
        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
        
        HasManyToMany(x => x.Tasks)
            .Table("task_tags")
            .ParentKeyColumn("tag_id")
            .ChildKeyColumn("task_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
        
        HasManyToMany(x => x.TimeEntries)
            .Table("time_entry_tags")
            .ParentKeyColumn("tag_id")
            .ChildKeyColumn("time_entry_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
    }
}
