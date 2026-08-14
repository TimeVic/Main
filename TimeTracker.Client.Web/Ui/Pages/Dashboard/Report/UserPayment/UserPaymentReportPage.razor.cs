using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Client.Core.Store.Report;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.UserPayment;

public partial class UserPaymentReportPage
{
    private readonly HashSet<Guid> _expandedClientIds = [];

    private UserPaymentReportResponse? _report => ReportsState.Value.UserPaymentReportData;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ReportFetchUserPaymentReportAction());
    }

    private void ToggleClient(Guid clientId)
    {
        if (!_expandedClientIds.Add(clientId))
        {
            _expandedClientIds.Remove(clientId);
        }
    }

    private string GetClientPaymentsUrl(Guid clientId)
    {
        return $"{UrlService.GetDashboardUrl("client-payments")}?clientId={clientId}";
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
        return outstanding > 0 ? "text-red-700" : "text-emerald-700";
    }

    private static string GetOutstandingCardClass(decimal outstanding)
    {
        return outstanding > 0
            ? "border-red-200 bg-red-50/40"
            : "border-emerald-200 bg-emerald-50/40";
    }

    private static string GetOutstandingBadgeClass(decimal outstanding)
    {
        return outstanding > 0
            ? "bg-red-100/80 text-red-700 border border-red-200"
            : "bg-emerald-100/80 text-emerald-700 border border-emerald-200";
    }
}
