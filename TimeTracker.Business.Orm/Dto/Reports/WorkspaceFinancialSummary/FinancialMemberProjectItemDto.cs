namespace TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;

public class FinancialMemberProjectItemDto
{
    public Guid MemberId { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public Guid? ClientId { get; set; }

    public string? ClientName { get; set; }

    public object DurationAsEpoch { get; set; } = null!;

    public object CostAmountOriginal { get; set; } = null!;

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));

    public decimal CostAmount => Convert.ToDecimal(CostAmountOriginal);
}
