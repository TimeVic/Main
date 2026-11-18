using TimeTracker.Business.Orm.Entities.Notes;

namespace TimeTracker.Business.Orm.Dto.Notes;

/// <summary>
/// Result of loading the visible note tree and private draft notes for a user.
/// </summary>
public class NoteTreeResultDto
{
    public ICollection<NoteNodeEntity> Nodes { get; set; } = new List<NoteNodeEntity>();

    public ICollection<NoteNodeEntity> Drafts { get; set; } = new List<NoteNodeEntity>();
}
