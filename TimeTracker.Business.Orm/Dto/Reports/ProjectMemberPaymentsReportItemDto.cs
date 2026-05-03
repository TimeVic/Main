using System.Reflection;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Dto.Reports;

public class ProjectMemberPaymentsReportItemDto
{
    public Guid? ProjectId { get; set; }
    
    public string? ProjectName { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public string? ClientName { get; set; }
    
    public object AmountOriginal { get; set; } = null!;
    
    public decimal Amount
    {
        get => Convert.ToDecimal(AmountOriginal);
    }
    
    public object PaidAmountByClientOriginal { get; set; } = null!;
    
    public decimal PaidAmountByClient
    {
        get => Convert.ToDecimal(PaidAmountByClientOriginal);
    }
    
    public object PaidAmountByProjectOriginal { get; set; } = null!;
    
    public decimal PaidAmountByProject
    {
        get => Convert.ToDecimal(PaidAmountByProjectOriginal);
    }
    
    public object TotalDurationAsEpoch { get; set; } = null!;
    
    public TimeSpan TotalDuration
    {
        get => TimeSpan.FromSeconds(
            Math.Round(Convert.ToDouble(TotalDurationAsEpoch))    
        );
    }
}
