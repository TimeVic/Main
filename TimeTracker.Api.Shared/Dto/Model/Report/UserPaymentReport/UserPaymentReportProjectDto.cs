namespace TimeTracker.Api.Shared.Dto.Model.Report.UserPaymentReport;

public class UserPaymentReportProjectDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }

    public decimal Earned { get; set; }
}
