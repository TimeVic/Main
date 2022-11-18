namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByDaysReportItemDto
{
    public DateTime Date { get; set; }

    public TimeSpan Duration { get; set; }

    public decimal Amount { get; set; }
    
    public double DurationAsMillis => Duration.TotalMilliseconds;
}
