using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class MemberPaymentReportResponse: IResponse
{
    public ICollection<MemberPaymentsReportItemDto> Items { get; set; } = null!;
}
