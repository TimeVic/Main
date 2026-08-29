using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Client.Core.Store.Report;
using WorkspaceMemberActions = TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.WorkspaceFinancialSummary;

public partial class WorkspaceFinancialSummaryPage
{
    private WorkspaceFinancialSummaryReportResponse? _reportData
        => _state.Value.WorkspaceFinancialSummaryData;

    private bool _isPayoutModalOpened;

    private Guid _selectedMemberId;
    private decimal? _selectedAmount;
    private Guid _selectedProjectId;

    private Guid _sharingClientId;
    private string _sharingClientName = string.Empty;
    private bool _isShareModalOpened;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new WorkspaceMemberActions.LoadListAction());
        Dispatcher.Dispatch(new ReportFetchWorkspaceFinancialSummaryAction());
    }

    private void OpenPayoutModal(WorkspaceFinancialMemberBalanceDto item)
    {
        _selectedMemberId = item.MemberId;
        _selectedAmount = item.Owed > 0 ? item.Owed : null;
        _selectedProjectId = item.Projects.Count == 1 ? item.Projects.First().Project.Id : Guid.Empty;
        _isPayoutModalOpened = true;
    }

    private void OnPayoutModalStateChanged(bool isOpened)
    {
        _isPayoutModalOpened = isOpened;
        if (!isOpened)
        {
            _selectedMemberId = Guid.Empty;
            _selectedAmount = null;
            _selectedProjectId = Guid.Empty;
            Dispatcher.Dispatch(new ReportFetchWorkspaceFinancialSummaryAction());
        }
    }

    private void OpenShareModal((Guid ClientId, string ClientName) client)
    {
        _sharingClientId = client.ClientId;
        _sharingClientName = client.ClientName;
        _isShareModalOpened = true;
    }

    private Task OnShareModalOpenedChanged(bool isOpened)
    {
        _isShareModalOpened = isOpened;
        return Task.CompletedTask;
    }
}
