using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.Notes;

public class NoteNodeHistoryEntity : AEntity
{
    public virtual required string Title { get; set; }
    public virtual int SortOrder { get; set; }

    #region Relationships

    public virtual required NoteNodeEntity NoteNode { get; set; }
    public virtual required NoteContentEntity Content { get; set; }

    #endregion
}
