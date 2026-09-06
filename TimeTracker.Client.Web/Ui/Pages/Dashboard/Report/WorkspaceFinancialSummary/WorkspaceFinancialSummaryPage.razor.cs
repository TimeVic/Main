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

    [Inject]
    private TimeTracker.Client.Web.Services.UI.IModalDialogProviderService _modalDialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new WorkspaceMemberActions.LoadListAction());
        Dispatcher.Dispatch(new ReportFetchWorkspaceFinancialSummaryAction());
    }

    private async Task OpenPayoutModal(WorkspaceFinancialMemberBalanceDto item)
    {
        var memberId = item.MemberId;
        var amount = item.Owed > 0 ? (decimal?)item.Owed : null;
        var projectId = item.Projects.Count == 1 ? item.Projects.First().Project.Id : Guid.Empty;

        await _modalDialogService.ShowAddMemberPaymentModal(
            initialMemberId: memberId,
            initialAmount: amount,
            initialProjectId: projectId,
            onClose: _ =>
            {
                Dispatcher.Dispatch(new ReportFetchWorkspaceFinancialSummaryAction());
            }
        );
    }

    private async Task OpenShareModal((Guid ClientId, string ClientName) client)
    {
        await _modalDialogService.ShowClientShareReportModal(client.ClientId, client.ClientName);
    }
}
