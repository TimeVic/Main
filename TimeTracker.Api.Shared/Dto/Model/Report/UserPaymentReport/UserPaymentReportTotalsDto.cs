namespace TimeTracker.Api.Shared.Dto.Model.Report.UserPaymentReport;

public class UserPaymentReportTotalsDto
{
    public decimal Earned { get; set; }

    public decimal Received { get; set; }

    public decimal Outstanding => Earned - Received;
}
