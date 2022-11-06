using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;

public class MembershipProjectAccessRequest
{
    [Required]
    [IsPositive]
    public long ProjectId { get; set; }

    [IsPositive]
    public decimal? HourlyRate { get; set; }
    
    public bool HasAccess { get; set; }
}
