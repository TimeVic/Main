namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class MemberPaymentsReportItemDto
{
    public Guid? ProjectId { get; set; }
    
    public string? ProjectName { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public string? ClientName { get; set; }
    
    public decimal Amount { get; set; }

    public decimal PaidAmountByClient { get; set; }
    
    public decimal PaidAmountByProject { get; set; }
    
    public TimeSpan TotalDuration { get; set; }
}
