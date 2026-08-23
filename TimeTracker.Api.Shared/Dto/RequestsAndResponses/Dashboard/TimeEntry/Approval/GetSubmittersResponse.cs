using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

public class GetSubmittersResponse : IResponse
{
    public IReadOnlyList<TimeEntryApprovalSubmitterDto> Items { get; set; } = new List<TimeEntryApprovalSubmitterDto>();
}
