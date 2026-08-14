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

    private static string GetStatusClass(decimal outstanding)
    {
        return outstanding > 0 ? "bg-red-100 text-red-700" : "bg-emerald-100 text-emerald-700";
    }

    private static string GetOutstandingTextClass(decimal outstanding)
    {
        return outstanding > 0 ? "text-red-700" : "text-emerald-700";
    }

    private static string GetOutstandingCardClass(decimal outstanding)
    {
        return outstanding > 0 ? "border-red-200 bg-red-50" : "border-emerald-100 bg-emerald-50/40";
    }
}
