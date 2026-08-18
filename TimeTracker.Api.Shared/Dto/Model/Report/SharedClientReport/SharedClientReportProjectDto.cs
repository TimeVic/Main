namespace TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;

public class SharedClientReportProjectDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }

    public decimal Earned { get; set; }
}
