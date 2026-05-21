using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetNoteDocumentRequest : IRequest<NoteDocumentDto>
{
    [RequiredNonEmpty]
    public Guid NoteId { get; set; }
}
