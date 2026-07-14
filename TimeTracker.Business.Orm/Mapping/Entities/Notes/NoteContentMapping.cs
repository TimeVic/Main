using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Notes;

public class NoteContentMapping : BaseGuidMappings<NoteContentEntity>
{
    public NoteContentMapping()
    {
        Table("note_contents");

        Map(x => x.MarkdownContent).Length(5_000_000);
        Map(x => x.CreatedAt).DateTime();

        References(x => x.NoteNode)
            .Column("note_node_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.None();
    }
}
