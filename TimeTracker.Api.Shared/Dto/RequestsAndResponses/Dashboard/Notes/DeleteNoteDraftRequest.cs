using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class DeleteNoteDraftRequest : IRequest
{
    [RequiredNonEmpty]
    public Guid NoteId { get; set; }
}
