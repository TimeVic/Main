using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity.Notes;

public class NoteNodeHistoryDto : IResponse
{
    public Guid Id { get; set; }

    public Guid NoteId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string MarkdownContent { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}
