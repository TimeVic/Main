using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class SummaryReportRequest: IRequest<SummaryReportResponse>
{
    [Required]
    [IsPositive]
    public long WorkspaceId { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }

    public SummaryReportType Type { get; set; }
}
