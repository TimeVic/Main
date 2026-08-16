using System.Globalization;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report.TeamSummary;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.Summary.Parts;

public partial class TeamSummaryChartSection
{
    [Parameter]
    public IEnumerable<TeamSummaryByDaysReportItemDto> Items { get; set; } = new List<TeamSummaryByDaysReportItemDto>();

    [Parameter]
    public string PeriodLabel { get; set; } = string.Empty;

    private decimal MaximumValue => Items.SelectMany(item => new[] { item.ClientBillable, item.TeamLaborCost }).DefaultIfEmpty().Max();

    private string GetBarStyle(decimal value)
    {
        if (MaximumValue <= 0 || value <= 0)
        {
            return "height:0%;";
        }

        var height = Math.Max((double)(value / MaximumValue * 100m), 8d);
        return $"height:{height.ToString("0.##", CultureInfo.InvariantCulture)}%;";
    }
}
