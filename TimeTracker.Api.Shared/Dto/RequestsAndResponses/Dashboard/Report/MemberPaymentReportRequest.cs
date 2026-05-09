using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class MemberPaymentReportRequest: IRequest<MemberPaymentReportResponse>
{
    public DateTime EndDate { get; set; }
}
