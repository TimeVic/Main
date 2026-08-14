namespace TimeTracker.Api.Shared.Dto.Model.Report.UserPaymentReport;

public class UserPaymentReportClientDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }

    public decimal Earned { get; set; }

    public decimal ProjectPayments { get; set; }

    public decimal GeneralPayments { get; set; }

    public decimal Received => ProjectPayments + GeneralPayments;

    public decimal Outstanding => Earned - Received;

    public ICollection<UserPaymentReportProjectDto> Projects { get; set; } = new List<UserPaymentReportProjectDto>();
}
