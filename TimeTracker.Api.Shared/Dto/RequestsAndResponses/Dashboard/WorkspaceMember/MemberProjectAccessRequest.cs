using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;

public class MemberProjectAccessRequest
{
    [RequiredNonEmpty]
    public Guid ProjectId { get; set; }

    [IsPositive(AllowZero = true)]
    public decimal? HourlyRate { get; set; }
    
    public bool HasAccess { get; set; }
}
