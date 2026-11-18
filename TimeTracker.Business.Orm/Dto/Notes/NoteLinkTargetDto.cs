using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Business.Orm.Dto.Notes;

/// <summary>
/// Target entity reference for linking a note document.
/// </summary>
public class NoteLinkTargetDto
{
    public NoteLinkEntityType EntityType { get; set; }

    public Guid EntityId { get; set; }
}
