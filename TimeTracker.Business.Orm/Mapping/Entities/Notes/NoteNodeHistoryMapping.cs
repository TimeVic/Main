using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Notes;

public class NoteNodeHistoryMapping : BaseGuidMappings<NoteNodeHistoryEntity>
{
    public NoteNodeHistoryMapping()
    {
        Table("note_node_history");

        Map(x => x.Title);
        Map(x => x.MarkdownContent).Length(5_000_000).Nullable();
        Map(x => x.SortOrder);
        Map(x => x.CreatedAt).DateTime();

        References(x => x.NoteNode)
            .Column("note_node_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
