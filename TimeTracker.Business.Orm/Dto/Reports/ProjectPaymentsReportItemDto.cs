using System.Reflection;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Dto.Reports;

public class ProjectPaymentsReportItemDto
{
    public long? ProjectId { get; set; }
    
    public string? ProjectName { get; set; }
    
    public long? ClientId { get; set; }
    
    public string? ClientName { get; set; }
    
    public object AmountOriginal { get; set; }
    public decimal Amount
    {
        get => Convert.ToDecimal(AmountOriginal);
    }
    
    public object PaidAmountByClientOriginal { get; set; }
    
    public decimal PaidAmountByClient
    {
        get => Convert.ToDecimal(PaidAmountByClientOriginal);
    }
    
    public object PaidAmountByProjectOriginal { get; set; }
    
    public decimal PaidAmountByProject
    {
        get => Convert.ToDecimal(PaidAmountByProjectOriginal);
    }
    
    public double TotalDurationAsEpoch { get; set; }
    
    public TimeSpan TotalDuration
    {
        get => TimeSpan.FromSeconds((double)TotalDurationAsEpoch);
    }
    
    public decimal UnpaidAmount
    {
        get => PaidAmountByClient - Amount;
    }
}
