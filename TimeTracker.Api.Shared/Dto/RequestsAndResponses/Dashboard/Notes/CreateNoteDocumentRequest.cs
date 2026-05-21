using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class CreateNoteDocumentRequest : IRequest<NoteDocumentDto>
{
    public Guid? ParentId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(5_000_000)]
    public string? MarkdownContent { get; set; }

    [Required]
    public NoteVisibility Visibility { get; set; }

    public int? SortOrder { get; set; }

    public ICollection<NoteLinkRequestDto>? Links { get; set; }
}
