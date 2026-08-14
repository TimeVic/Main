namespace TimeTracker.Business.Orm.Dto.Reports;

public class UserPaymentReportClientPaymentItemDto
{
    public Guid ClientId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public object ProjectPaymentsOriginal { get; set; } = null!;

    public object GeneralPaymentsOriginal { get; set; } = null!;

    public decimal ProjectPayments => Convert.ToDecimal(ProjectPaymentsOriginal);

    public decimal GeneralPayments => Convert.ToDecimal(GeneralPaymentsOriginal);
}
