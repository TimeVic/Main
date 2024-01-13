using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;

public class GetRequest : IRequest<GoalsTrackerDto>
{
    [Required]
    [IsPositive]
    public long WorkspaceId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
}
