using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class DeleteNoteLinkRequest : IRequest
{
    [RequiredNonEmpty]
    public Guid NoteId { get; set; }

    [RequiredNonEmpty]
    public Guid LinkId { get; set; }
}
