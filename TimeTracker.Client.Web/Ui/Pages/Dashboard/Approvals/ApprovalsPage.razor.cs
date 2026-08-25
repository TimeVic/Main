using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.TimeEntry.Approvals;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Approvals;

public partial class ApprovalsPage
{
    private bool _isRejectModalOpened;
    private ICollection<Guid> _pendingRejectEntryIds = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Redirect regular users or non-team workspaces
        if (!IsAuthorizedManager)
        {
            NavigationManager.NavigateTo(UrlService.GetDashboardUrl());
            return;
        }

        Dispatcher.Dispatch(new FetchSubmittersAction());
    }

    private bool IsAuthorizedManager =>
        AuthState.Value.Workspace?.Mode == WorkspaceMode.Team
        && AuthState.Value.IsRoleAdmin;

    private void OnSubmitterSelected(TimeEntryApprovalSubmitterDto submitter)
    {
        Dispatcher.Dispatch(new SelectSubmitterAction(submitter));
        Dispatcher.Dispatch(new FetchApprovalDetailsAction(
            submitter.UserId,
            submitter.PeriodStartDate,
            submitter.PeriodEndDate
        ));
    }

    private void OnApproveEntries(ICollection<Guid> entryIds)
    {
        Dispatcher.Dispatch(new ApproveEntriesAction(entryIds));
    }

    private void OnOpenRejectModal(ICollection<Guid> entryIds)
    {
        _pendingRejectEntryIds = entryIds;
        _isRejectModalOpened = true;
    }

    private void OnConfirmReject(string reason)
    {
        _isRejectModalOpened = false;
        if (_pendingRejectEntryIds.Any())
        {
            Dispatcher.Dispatch(new RejectEntriesAction(_pendingRejectEntryIds, reason));
            _pendingRejectEntryIds = [];
        }
    }

    private void OnUnapproveEntries(ICollection<Guid> entryIds)
    {
        Dispatcher.Dispatch(new UnapproveEntriesAction(entryIds));
    }
}
