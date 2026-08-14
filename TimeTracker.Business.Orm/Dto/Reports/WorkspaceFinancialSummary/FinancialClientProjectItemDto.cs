namespace TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;

public class FinancialClientProjectItemDto
{
    public Guid ClientId { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public object DurationAsEpoch { get; set; } = null!;

    public object EarnedAmountOriginal { get; set; } = null!;

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));

    public decimal EarnedAmount => Convert.ToDecimal(EarnedAmountOriginal);
}
