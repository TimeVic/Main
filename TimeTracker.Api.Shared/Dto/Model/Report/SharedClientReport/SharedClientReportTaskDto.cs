namespace TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;

public class SharedClientReportTaskDto
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }
}
