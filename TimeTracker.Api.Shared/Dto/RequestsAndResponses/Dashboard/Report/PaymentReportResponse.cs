using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class PaymentReportResponse: IResponse
{
    public ICollection<PaymentsReportItemDto> Items { get; set; }
}
