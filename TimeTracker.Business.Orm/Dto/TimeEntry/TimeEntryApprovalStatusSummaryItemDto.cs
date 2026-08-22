namespace TimeTracker.Business.Orm.Dto.TimeEntry;

public class TimeEntryApprovalStatusSummaryItemDto
{
    public int DraftCount { get; set; }
    public double DraftDurationSeconds { get; set; }
    public decimal DraftAmount { get; set; }
    public int PendingCount { get; set; }
    public double PendingDurationSeconds { get; set; }
    public decimal PendingAmount { get; set; }
    public int RejectedCount { get; set; }
    public string? LatestRejectionReason { get; set; }
}
