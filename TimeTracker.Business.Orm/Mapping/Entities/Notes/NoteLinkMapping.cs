using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities.Notes;

public class NoteLinkMapping : BaseGuidMappings<NoteLinkEntity>
{
    public NoteLinkMapping()
    {
        Table("note_links");

        Map(x => x.EntityType).Enum<NoteLinkEntityType>();
        Map(x => x.EntityId);
        Map(x => x.CreatedAt).DateTime();

        References(x => x.Workspace)
            .Column("workspace_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();

        References(x => x.NoteNode)
            .Column("note_node_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();

        References(x => x.CreatedByUser)
            .Column("created_by_user_id")
            .Fetch.Select()
            .LazyLoad()
            .Cascade.SaveUpdate();
    }
}
