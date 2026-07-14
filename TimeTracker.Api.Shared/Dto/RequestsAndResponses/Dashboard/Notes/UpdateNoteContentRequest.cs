using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class UpdateNoteContentRequest : IRequest<NoteContentDto>
{
    [RequiredNonEmpty]
    public Guid NoteId { get; set; }

    [StringLength(5_000_000)]
    public string? MarkdownContent { get; set; }
}
