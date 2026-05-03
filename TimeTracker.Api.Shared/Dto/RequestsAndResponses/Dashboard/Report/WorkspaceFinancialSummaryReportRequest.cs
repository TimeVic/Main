using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class WorkspaceFinancialSummaryReportRequest : IRequest<WorkspaceFinancialSummaryReportResponse>
{
    [Required]
    public Guid WorkspaceId { get; set; }
}
