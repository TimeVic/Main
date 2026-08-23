using Fluxor;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals;

[FeatureState]
public record ApprovalsState
{
    public bool IsLoading { get; init; }
    public bool IsDetailsLoading { get; init; }
    public bool IsActionProcessing { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<TimeEntryApprovalSubmitterDto> Submitters { get; init; } = [];
    public TimeEntryApprovalSubmitterDto? SelectedSubmitter { get; init; }
    public GetApprovalDetailsResponse? Details { get; init; }
}
