using TimeTracker.Api.Shared.Dto.Entity.Notes;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Models;

public class NoteTreeNodeModel
{
    public required NoteTreeNodeDto Node { get; init; }

    public IReadOnlyList<NoteTreeNodeModel> Children { get; set; } = Array.Empty<NoteTreeNodeModel>();
}
