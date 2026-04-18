namespace TimeTracker.Business.Orm.Dto.Reports.Summary;

public class ByDaysReportItemDto
{
    public DateTime Date { get; set; }
    
    public object DurationAsEpoch { get; set; } = null!;
    
    public object AmountOriginal { get; set; } = null!;

    public TimeSpan Duration
    {
        get => TimeSpan.FromSeconds(
            Math.Round(Convert.ToDouble(DurationAsEpoch))
        );
    }
    
    public decimal Amount
    {
        get => Convert.ToDecimal(AmountOriginal);
    }
}
