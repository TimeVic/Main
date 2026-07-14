using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Notes;

public class NoteNodeMapping : BaseGuidMappings<NoteNodeEntity>
{
    public NoteNodeMapping()
    {
        Table("note_nodes");

        Map(x => x.Type).Enum<NoteNodeType>();
        Map(x => x.Title);
        Map(x => x.SortOrder);
        Map(x => x.Visibility).Enum<NoteVisibility>();
        Map(x => x.ArchivedAt).DateTimeNullable();
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTimeNullable();

        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();

        References(x => x.Parent)
            .Column("parent_id")
            .Fetch.Select()
            .LazyLoad()
            .Nullable()
            .Cascade.None();

        References(x => x.CreatedByUser)
            .Column("created_by_user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();

        References(x => x.UpdatedByUser)
            .Column("updated_by_user_id")
            .Fetch.Select()
            .LazyLoad()
            .Nullable()
            .Cascade.SaveUpdate();

        References(x => x.LastContent)
            .Column("last_content_id")
            .Fetch.Select()
            .LazyLoad()
            .Nullable()
            .Cascade.None();

        HasMany(x => x.Children)
            .KeyColumn("parent_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate()
            .Inverse();

        HasMany(x => x.Links)
            .KeyColumn("note_node_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.AllDeleteOrphan()
            .Inverse();

        HasMany(x => x.History)
            .KeyColumn("note_node_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.AllDeleteOrphan()
            .Inverse();
    }
}
