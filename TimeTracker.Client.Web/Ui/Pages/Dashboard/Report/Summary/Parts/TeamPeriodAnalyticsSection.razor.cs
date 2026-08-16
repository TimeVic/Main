using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report.TeamSummary;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.Summary.Parts;

public partial class TeamPeriodAnalyticsSection
{
    [Parameter]
    public TeamSummaryTotalsDto? Totals { get; set; }

    private decimal GrossProfit => Totals?.GrossProfit ?? 0m;

    private string ProfitVariant => GrossProfit < 0 ? "danger" : "success";
}
