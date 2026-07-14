using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.Notes;

public class NoteNodeEntity : AEntity
{
    public virtual NoteNodeType Type { get; set; }
    public virtual required string Title { get; set; }
    public virtual int SortOrder { get; set; }
    public virtual NoteVisibility Visibility { get; set; }
    public virtual DateTime? ArchivedAt { get; set; }

    #region Relationships

    public virtual required WorkspaceEntity Workspace { get; set; }
    public virtual NoteNodeEntity? Parent { get; set; }
    public virtual required UserEntity CreatedByUser { get; set; }
    public virtual UserEntity? UpdatedByUser { get; set; }
    public virtual NoteContentEntity? LastContent { get; set; }
    public virtual ICollection<NoteNodeEntity> Children { get; set; } = new List<NoteNodeEntity>();
    public virtual ICollection<NoteLinkEntity> Links { get; set; } = new List<NoteLinkEntity>();
    public virtual ICollection<NoteNodeHistoryEntity> History { get; set; } = new List<NoteNodeHistoryEntity>();

    #endregion

    #region Calculated

    public virtual bool IsArchived => ArchivedAt.HasValue;

    #endregion
}
