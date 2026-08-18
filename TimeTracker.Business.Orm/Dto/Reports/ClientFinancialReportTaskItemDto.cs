namespace TimeTracker.Business.Orm.Dto.Reports;

public class ClientFinancialReportTaskItemDto
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public object DurationAsEpoch { get; set; } = null!;

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));
}


