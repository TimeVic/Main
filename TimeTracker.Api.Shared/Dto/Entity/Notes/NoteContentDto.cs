using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity.Notes;

public class NoteContentDto : IResponse
{
    public Guid Id { get; set; }

    public Guid NoteId { get; set; }

    public string MarkdownContent { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
