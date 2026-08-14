namespace TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;

public class FinancialProjectProfitabilityItemDto
{
    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public Guid? ClientId { get; set; }

    public string? ClientName { get; set; }

    public object DurationAsEpoch { get; set; } = null!;

    public object EarnedAmountOriginal { get; set; } = null!;

    public object TeamCostAmountOriginal { get; set; } = null!;

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));

    public decimal EarnedAmount => Convert.ToDecimal(EarnedAmountOriginal);

    public decimal TeamCostAmount => Convert.ToDecimal(TeamCostAmountOriginal);

    public decimal? ClientHourlyRate => Duration == TimeSpan.Zero
        ? null
        : Math.Round(EarnedAmount / (decimal)Duration.TotalHours, 2);

    public decimal? TeamHourlyRate => Duration == TimeSpan.Zero
        ? null
        : Math.Round(TeamCostAmount / (decimal)Duration.TotalHours, 2);

    public decimal EstimatedMargin => EarnedAmount - TeamCostAmount;

    public decimal? MarginPercent => EarnedAmount == 0 ? null : Math.Round(EstimatedMargin / EarnedAmount * 100, 1);
}
