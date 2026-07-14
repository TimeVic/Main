using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.Notes;

public class NoteContentEntity : AEntity
{
    public virtual required string MarkdownContent { get; set; }

    #region Relationships

    public virtual required NoteNodeEntity NoteNode { get; set; }

    #endregion
}
