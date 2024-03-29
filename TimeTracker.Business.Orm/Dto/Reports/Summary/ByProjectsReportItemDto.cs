﻿namespace TimeTracker.Business.Orm.Dto.Reports.Summary;

public class ByProjectsReportItemDto
{
    public long? ProjectId { get; set; }
    
    public string? ProjectName { get; set; }
    
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
