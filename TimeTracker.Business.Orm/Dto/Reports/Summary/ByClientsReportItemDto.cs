namespace TimeTracker.Business.Orm.Dto.Reports.Summary;

public class ByClientsReportItemDto
{
    public long? ClientId { get; set; }
    
    public string? ClientName { get; set; }
    
    public object DurationAsEpoch { get; set; }

    public TimeSpan Duration
    {
        get => TimeSpan.FromSeconds(
            Convert.ToDouble(DurationAsEpoch)
        );
    }
}
