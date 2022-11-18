namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByMonthsReportItemDto
{
    public int Month { get; set; }
    
    public int Year { get; set; }

    public decimal Amount { get; set; }
    
    public TimeSpan Duration { get; set; }
}
