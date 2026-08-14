using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.Report.UserPaymentReport;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class UserPaymentReportResponse : IResponse
{
    public UserPaymentReportTotalsDto Totals { get; set; } = new();

    public ICollection<UserPaymentReportClientDto> Clients { get; set; } = new List<UserPaymentReportClientDto>();
}
