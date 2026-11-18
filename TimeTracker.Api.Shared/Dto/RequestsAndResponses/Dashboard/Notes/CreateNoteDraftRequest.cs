using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class CreateNoteDraftRequest : IRequest<NoteDocumentDto>
{
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(5_000_000)]
    public string? MarkdownContent { get; set; }
}
