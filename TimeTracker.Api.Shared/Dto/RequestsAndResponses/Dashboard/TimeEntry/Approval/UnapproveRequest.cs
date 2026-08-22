using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

public class UnapproveRequest : IRequest<TimeEntryDto>
{
    [Required]
    public Guid TimeEntryId { get; set; }
}
