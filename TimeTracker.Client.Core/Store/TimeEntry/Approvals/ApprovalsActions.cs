using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals;

public record struct FetchSubmittersAction();

public record struct SetSubmittersAction(GetSubmittersResponse Response);

public record struct SelectSubmitterAction(TimeEntryApprovalSubmitterDto? Submitter);

public record struct FetchApprovalDetailsAction(Guid UserId, DateTime StartDate, DateTime EndDate);

public record struct SetApprovalDetailsAction(GetApprovalDetailsResponse? Details);

public record struct SetIsLoadingAction(bool IsLoading);

public record struct SetIsDetailsLoadingAction(bool IsLoading);

public record struct SetIsActionProcessingAction(bool IsProcessing);

public record struct SetErrorMessageAction(string? ErrorMessage);

public record struct ApproveEntriesAction(ICollection<Guid> TimeEntryIds);

public record struct RejectEntriesAction(ICollection<Guid> TimeEntryIds, string Reason);

public record struct UnapproveEntriesAction(ICollection<Guid> TimeEntryIds);
