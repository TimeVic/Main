using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetNoteContentRequest : IRequest<NoteContentDto>
{
    [RequiredNonEmpty]
    public Guid ContentId { get; set; }
}
