using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

public class RejectRequest : IRequest<PaginatedListDto<TimeEntryDto>>
{
    public ICollection<Guid> TimeEntryIds { get; set; } = new List<Guid>();

    [Required]
    public string Reason { get; set; } = string.Empty;
}
