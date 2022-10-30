namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByWeeksReportItemDto
{
    public int Week { get; set; }

    public TimeSpan Duration { get; set; }
}
