namespace TimeTracker.Business.Orm.Dto.Reports.Summary;

public class ByWeeksReportItemDto
{
    public int Week { get; set; }
    
    public object DurationAsEpoch { get; set; }

    public TimeSpan Duration
    {
        get => TimeSpan.FromSeconds(
            Convert.ToDouble(DurationAsEpoch)
        );
    }
}
