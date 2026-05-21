using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.Notes;

public class NoteLinkEntity : AEntity
{
    public virtual NoteLinkEntityType EntityType { get; set; }
    public virtual Guid EntityId { get; set; }

    #region Relationships

    public virtual required WorkspaceEntity Workspace { get; set; }
    public virtual required NoteNodeEntity NoteNode { get; set; }
    public virtual required UserEntity CreatedByUser { get; set; }

    #endregion
}
