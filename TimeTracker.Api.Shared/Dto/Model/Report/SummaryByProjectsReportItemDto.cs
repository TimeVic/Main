namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByProjectsReportItemDto
{
    public long? ProjectId { get; set; }

    public TimeSpan Duration { get; set; }
}
