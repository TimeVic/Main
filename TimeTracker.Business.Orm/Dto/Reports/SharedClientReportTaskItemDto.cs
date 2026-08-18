namespace TimeTracker.Business.Orm.Dto.Reports;

public class SharedClientReportTaskItemDto
{
    public Guid ProjectId { get; set; }

    public Guid TaskId { get; set; }

    public string TaskTitle { get; set; } = string.Empty;

    public object DurationAsEpoch { get; set; } = null!;

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));
}
