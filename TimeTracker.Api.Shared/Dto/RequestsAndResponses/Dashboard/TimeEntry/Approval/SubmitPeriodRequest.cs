using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Common.Attributes;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

public class SubmitPeriodRequest : IRequest<PaginatedListDto<TimeEntryDto>>
{
    [Required]
    [NonConvertibleDateTime]
    public DateTime StartDate { get; set; }

    [Required]
    [NonConvertibleDateTime]
    public DateTime EndDate { get; set; }
}
