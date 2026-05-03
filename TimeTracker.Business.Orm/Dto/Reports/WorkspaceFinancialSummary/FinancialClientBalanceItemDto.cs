namespace TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;

public class FinancialClientBalanceItemDto
{
    public Guid ClientId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public object DurationAsEpoch { get; set; } = null!;

    public object EarnedAmountOriginal { get; set; } = null!;

    public object ReceivedAmountOriginal { get; set; } = null!;

    public object? LastPaymentDateRaw { get; set; }

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));

    public decimal EarnedAmount => Convert.ToDecimal(EarnedAmountOriginal);

    public decimal ReceivedAmount => Convert.ToDecimal(ReceivedAmountOriginal);

    public decimal OutstandingAmount => EarnedAmount - ReceivedAmount;

    public DateTime? LastPaymentDate => LastPaymentDateRaw == null ? null : Convert.ToDateTime(LastPaymentDateRaw);
}
