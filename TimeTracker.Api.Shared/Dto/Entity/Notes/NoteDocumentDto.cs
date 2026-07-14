using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Api.Shared.Dto.Entity.Notes;

public class NoteDocumentDto : IResponse
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public Guid? LastContentId { get; set; }

    public NoteVisibility Visibility { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<NoteLinkDto> Links { get; set; } = new List<NoteLinkDto>();
}
