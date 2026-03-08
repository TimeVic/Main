using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class PaymentReportRequest: IRequest<PaymentReportResponse>
{
    [Required]
    public Guid WorkspaceId { get; set; }
    
    public DateTime EndDate { get; set; }
}
