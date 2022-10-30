namespace TimeTracker.Business.Orm.Dto.Reports.Summary;

public class ByDaysReportItemDto
{
    public DateTime Date { get; set; }
    
    public object DurationAsEpoch { get; set; }

    public TimeSpan Duration
    {
        get => TimeSpan.FromSeconds(
            Convert.ToDouble(DurationAsEpoch)
        );
    }
}
