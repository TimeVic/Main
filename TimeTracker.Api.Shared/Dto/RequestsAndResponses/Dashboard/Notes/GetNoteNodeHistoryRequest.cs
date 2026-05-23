using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetNoteNodeHistoryRequest : IRequest<GetNoteNodeHistoryResponse>
{
    [RequiredNonEmpty]
    public Guid NoteId { get; set; }
}
