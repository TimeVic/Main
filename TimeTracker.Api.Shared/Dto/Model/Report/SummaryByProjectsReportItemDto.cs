namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByProjectsReportItemDto
{
    public long? ProjectId { get; set; }
    
    public string? ProjectName { get; set; }

    public decimal Amount { get; set; }
    
    public TimeSpan Duration { get; set; }
}
