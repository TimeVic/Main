using Api.Requests.Abstractions;

namespace TimeTracker.Business.Common.Dto;

public class TimeEntryApprovalStatusSummaryDto : IResponse
{
    public TimeSpan DraftDuration { get; set; }
    public decimal DraftAmount { get; set; }
    public int DraftCount { get; set; }
    
    public TimeSpan PendingDuration { get; set; }
    public decimal PendingAmount { get; set; }
    public int PendingCount { get; set; }

    public TimeSpan PendingAndDraftDuration => PendingDuration + DraftDuration;
    public decimal PendingAndDraftAmount => PendingAmount + DraftAmount;

    public int RejectedCount { get; set; }
    public string? LatestRejectionReason { get; set; }

    public bool HasDraftEntries => DraftCount > 0;
    public bool HasRejectedEntries => RejectedCount > 0;
}
