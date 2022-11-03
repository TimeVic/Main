using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class PaymentReportRequest: IRequest<PaymentReportResponse>
{
    [Required]
    [IsPositive]
    public long WorkspaceId { get; set; }
    
    public DateTime EndDate { get; set; }
}
