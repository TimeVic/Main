namespace TimeTracker.Business.Orm.Dto.TimeEntry;

public class TimeEntryApprovalSubmitterItemDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public double TotalDurationSeconds { get; set; }
    public decimal TotalDeveloperAmount { get; set; }
    public decimal TotalClientAmount { get; set; }
    public int PendingCount { get; set; }
}
