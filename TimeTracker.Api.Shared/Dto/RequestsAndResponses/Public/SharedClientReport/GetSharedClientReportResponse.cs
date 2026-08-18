using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

public class GetSharedClientReportResponse : IResponse
{
    public string ClientName { get; set; } = string.Empty;

    public string WorkspaceName { get; set; } = string.Empty;

    public string? CurrencyCode { get; set; }

    public bool IsShowTasks { get; set; }

    public SharedClientReportTotalsDto Totals { get; set; } = new();

    public ICollection<SharedClientReportProjectDto> Projects { get; set; } = new List<SharedClientReportProjectDto>();

    public ICollection<SharedClientReportPaymentDto> Payments { get; set; } = new List<SharedClientReportPaymentDto>();
}
