namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByWeeksReportItemDto
{
    public DateTime WeekStartDate { get; set; }
    
    public DateTime WeekEndDate { get; set; }

    public TimeSpan Duration { get; set; }
    
    public decimal Amount { get; set; }
}
