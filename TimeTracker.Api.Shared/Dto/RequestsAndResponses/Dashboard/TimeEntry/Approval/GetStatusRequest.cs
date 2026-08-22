using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

public class GetStatusRequest : IRequest<TimeEntryApprovalStatusSummaryDto>
{
}
