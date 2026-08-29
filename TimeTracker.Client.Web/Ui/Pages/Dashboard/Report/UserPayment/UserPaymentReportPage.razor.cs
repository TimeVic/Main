using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Api.Shared.Dto.Model.Report.UserPaymentReport;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Report;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.UserPayment;

public partial class UserPaymentReportPage
{
    private readonly HashSet<Guid> _expandedClientIds = [];
    private Guid _sharingClientId;
    private string _sharingClientName = string.Empty;
    private bool _isShareModalOpened;

    private UserPaymentReportResponse? _report => ReportsState.Value.UserPaymentReportData;

    private UserPaymentReportFilterState _filterState => ReportsState.Value.UserPaymentReportFilter;

    private bool IsCanShareClientReport => AuthState.Value.Workspace?.IsWorkspaceAdmin == true;
    private bool IsTeamWorkspace => AuthState.Value.Workspace?.Mode == WorkspaceMode.Team;

    private TimeTracker.Business.Common.Dto.TimeEntryApprovalStatusSummaryDto? _approvalSummary;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ReportFetchUserPaymentReportAction());

        if (IsTeamWorkspace)
        {
            try
            {
                _approvalSummary = await ApiService.TimeEntryApprovalGetStatusAsync();
            }
            catch
            {
                // Background error ignored
            }
        }
    }

    private void ToggleClient(Guid clientId)
    {
        if (!_expandedClientIds.Add(clientId))
        {
            _expandedClientIds.Remove(clientId);
        }
    }

    private void OpenShareModal(UserPaymentReportClientDto client)
    {
        _sharingClientId = client.Id;
        _sharingClientName = client.Name;
        _isShareModalOpened = true;
    }

    private Task OnShareModalOpenedChanged(bool isOpened)
    {
        _isShareModalOpened = isOpened;
        return Task.CompletedTask;
    }

    private string GetClientPaymentsUrl(Guid clientId)
    {
        if (_report?.IsPaymentsFromMembers == true)
        {
            return UrlService.GetDashboardUrl("member-payments");
        }

        return $"{UrlService.GetDashboardUrl("client-payments")}?clientId={clientId}";
    }

    private void OnEndDateChanged(DateTime? endDate)
    {
        if (endDate == null || _filterState.EndDate.Date == endDate.Value.Date)
        {
            return;
        }

        Dispatcher.Dispatch(new ReportSetUserPaymentReportFilterAction(new UserPaymentReportFilterState(endDate.Value)));
        Dispatcher.Dispatch(new ReportFetchUserPaymentReportAction());
    }

    private string GetStatus(decimal outstanding)
    {
        return outstanding > 0
            ? DashboardLocalizer["UserPaymentReport_Debt"].Value
            : DashboardLocalizer["UserPaymentReport_Paid"].Value;
    }

    private string GetOutstandingBadgeText(decimal outstanding)
    {
        if (outstanding > 0)
        {
            return DashboardLocalizer["UserPaymentReport_UnpaidDebt"].Value;
        }

        if (outstanding < 0)
        {
            return DashboardLocalizer["UserPaymentReport_Surplus"].Value;
        }

        return DashboardLocalizer["UserPaymentReport_FullySettled"].Value;
    }

    private string GetOutstandingBadgeLabel(decimal outstanding)
    {
        if (outstanding > 0)
        {
            return DashboardLocalizer["UserPaymentReport_ToPayout"].Value;
        }

        if (outstanding < 0)
        {
            return DashboardLocalizer["UserPaymentReport_Prepaid"].Value;
        }

        return DashboardLocalizer["UserPaymentReport_FullySettled"].Value;
    }

    private static string GetStatusBadgeClass(decimal outstanding)
    {
        return outstanding > 0
            ? "bg-red-50 text-red-700 border border-red-200/80"
            : "bg-emerald-50 text-emerald-700 border border-emerald-200/80";
    }

    private static string GetStatusDotClass(decimal outstanding)
    {
        return outstanding > 0 ? "bg-red-500" : "bg-emerald-500";
    }

    private static string GetOutstandingTextClass(decimal outstanding)
    {
        if (outstanding > 0)
        {
            return "text-emerald-600";
        }

        return "text-slate-900";
    }

    private static string GetOutstandingCardClass(decimal outstanding)
    {
        if (outstanding < 0)
        {
            return "border border-amber-200/80 bg-amber-50/40";
        }

        return "border border-slate-200 bg-white";
    }

    private static string GetOutstandingBadgeClass(decimal outstanding)
    {
        if (outstanding > 0)
        {
            return "bg-emerald-50 text-emerald-700 border border-emerald-200/60";
        }

        if (outstanding < 0)
        {
            return "bg-amber-100 text-amber-800 border border-amber-200";
        }

        return "bg-slate-100 text-slate-600 border border-slate-200";
    }
}
