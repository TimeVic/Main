﻿namespace TimeTracker.Business.Orm.Dto.Reports.Summary;

public class ByMonthsReportItemDto
{
    public int Month { get; set; }
    
    public int Year { get; set; }
    
    public object DurationAsEpoch { get; set; }
    
    public object AmountOriginal { get; set; }

    public TimeSpan Duration
    {
        get => TimeSpan.FromSeconds(
            Convert.ToDouble(DurationAsEpoch)
        );
    }
    
    public decimal Amount
    {
        get => Convert.ToDecimal(AmountOriginal);
    }
}
