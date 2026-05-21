using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class NoteLinkRequestDto
{
    [Required]
    public NoteLinkEntityType EntityType { get; set; }

    [RequiredNonEmpty]
    public Guid EntityId { get; set; }
}
