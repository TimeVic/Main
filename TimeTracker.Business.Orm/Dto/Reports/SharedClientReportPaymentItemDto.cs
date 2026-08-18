namespace TimeTracker.Business.Orm.Dto.Reports;

public class SharedClientReportPaymentItemDto
{
    public DateTime PaymentTime { get; set; }

    public object AmountOriginal { get; set; } = null!;

    public string? ProjectName { get; set; }

    public string? Description { get; set; }

    public decimal Amount => Convert.ToDecimal(AmountOriginal);
}
