namespace TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;

public class FinancialMemberBalanceItemDto
{
    public Guid MemberId { get; set; }

    public Guid UserId { get; set; }

    public string? UserName { get; set; }

    public string Email { get; set; } = string.Empty;

    public object DurationAsEpoch { get; set; } = null!;

    public object CostAmountOriginal { get; set; } = null!;

    public object PaidOutAmountOriginal { get; set; } = null!;

    public object? LastPayoutDateRaw { get; set; }

    public string DisplayName => string.IsNullOrEmpty(UserName) ? Email : UserName;

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));

    public decimal CostAmount => Convert.ToDecimal(CostAmountOriginal);

    public decimal PaidOutAmount => Convert.ToDecimal(PaidOutAmountOriginal);

    public decimal OwedAmount => CostAmount - PaidOutAmount;

    public DateTime? LastPayoutDate => LastPayoutDateRaw == null ? null : Convert.ToDateTime(LastPayoutDateRaw);
}
