using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;

public class ChangePositionsRequest : IRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }

    [Required]
    public IDictionary<Guid, int> Positions { get; set; } = new Dictionary<Guid, int>();
}
