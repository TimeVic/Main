namespace TimeTracker.Business.Orm.Dto.Reports.Summary;

public class ByUsersReportItemDto
{
    public long? UserId { get; set; }
    
    public object DurationAsEpoch { get; set; }

    public TimeSpan Duration
    {
        get => TimeSpan.FromSeconds(
            Convert.ToDouble(DurationAsEpoch)
        );
    }
}
