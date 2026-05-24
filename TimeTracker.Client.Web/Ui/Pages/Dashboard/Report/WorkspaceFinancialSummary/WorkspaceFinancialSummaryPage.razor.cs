using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Report;
using TimeTracker.Client.Core.Services.Security;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.WorkspaceFinancialSummary;

public partial class WorkspaceFinancialSummaryPage
{
    [Inject]
    public IState<AuthState> _authState { get; set; } = null!;

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = null!;

    private WorkspaceFinancialSummaryReportResponse? _reportData
        => _state.Value.WorkspaceFinancialSummaryData;

    private bool _isAuthorized
        => SecurityManager.HasPermission(WorkspacePermission.ReadWorkspaceFinancialSummary);

    private bool _showMemberPayouts
        => _reportData?.HasMemberPayouts == true;

    private int _unpaidClientCount
        => _reportData?.ClientBalances.Count(x => x.Outstanding > 0) ?? 0;

    private decimal _unpaidClientAmount
        => _reportData?.ClientBalances.Where(x => x.Outstanding > 0).Sum(x => x.Outstanding) ?? 0;

    private int _membersToPayCount
        => _reportData?.MemberBalances.Count(x => x.Owed > 0) ?? 0;

    private decimal _membersToPayAmount
        => _reportData?.MemberBalances.Where(x => x.Owed > 0).Sum(x => x.Owed) ?? 0;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (_isAuthorized)
        {
            Dispatcher.Dispatch(new ReportFetchWorkspaceFinancialSummaryAction());
        }
    }
}
