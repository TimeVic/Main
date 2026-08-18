namespace TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;

public class SharedClientReportPaymentDto
{
    public DateTime PaymentTime { get; set; }

    public decimal Amount { get; set; }

    public string? ProjectName { get; set; }

    public string? Description { get; set; }
}
