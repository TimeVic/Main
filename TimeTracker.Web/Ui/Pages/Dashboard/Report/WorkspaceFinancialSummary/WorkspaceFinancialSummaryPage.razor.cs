using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Report;
using TimeTracker.Web.Services.Security;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report.WorkspaceFinancialSummary;

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

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (_isAuthorized)
        {
            Dispatcher.Dispatch(new ReportFetchWorkspaceFinancialSummaryAction());
        }
    }
}
