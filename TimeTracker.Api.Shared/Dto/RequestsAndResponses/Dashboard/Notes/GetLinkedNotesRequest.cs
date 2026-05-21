using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetLinkedNotesRequest : IRequest<GetLinkedNotesResponse>
{
    [Required]
    public NoteLinkEntityType EntityType { get; set; }

    [RequiredNonEmpty]
    public Guid EntityId { get; set; }
}
