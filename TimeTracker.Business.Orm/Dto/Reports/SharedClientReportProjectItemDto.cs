namespace TimeTracker.Business.Orm.Dto.Reports;

public class SharedClientReportProjectItemDto
{
    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public object DurationAsEpoch { get; set; } = null!;

    public object EarnedOriginal { get; set; } = null!;

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));

    public decimal Earned => Convert.ToDecimal(EarnedOriginal);
}
