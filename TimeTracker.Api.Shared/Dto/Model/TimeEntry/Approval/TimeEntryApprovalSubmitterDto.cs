using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;

public class TimeEntryApprovalSubmitterDto : IResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public int WeekNumber { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public decimal TotalDeveloperAmount { get; set; }
    public decimal TotalClientAmount { get; set; }
    public int PendingCount { get; set; }
    public TimeEntryStatus Status { get; set; }
    public bool IsCurrentUser { get; set; }
}
