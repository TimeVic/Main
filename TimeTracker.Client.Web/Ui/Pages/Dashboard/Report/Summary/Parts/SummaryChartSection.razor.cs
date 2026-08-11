using Microsoft.AspNetCore.Components;
using System.Globalization;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Client.Core.Core.Components;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.Summary.Parts;

public partial class SummaryChartSection : BaseReactiveComponent
{
    [Inject]
    public ITimeParsingService TimeParsingService { get; set; } = default!;

    [Parameter]
    public IEnumerable<SummaryByDaysReportItemDto> Items { get; set; } = new List<SummaryByDaysReportItemDto>();

    [Parameter]
    public string PeriodLabel { get; set; } = string.Empty;

    private int _trackedDaysCount
    {
        get => Items.Count(item => item.Duration > TimeSpan.Zero || item.Amount > 0);
    }

    private decimal _totalEarned
    {
        get => Items.Sum(item => item.Amount);
    }

    private string _trackedDaysLabel
    {
        get => _trackedDaysCount == 1
            ? DashboardLocalizer["OneDayTracked"].Value
            : string.Format(DashboardLocalizer["DaysTracked"].Value, _trackedDaysCount);
    }

    private string GetDurationBarStyle(SummaryByDaysReportItemDto item)
    {
        var maxValue = Items.Any() ? Items.Max(chartItem => chartItem.DurationAsMillis) : 0;
        if (maxValue <= 0 || item.DurationAsMillis <= 0)
        {
            return "height:0%;";
        }

        var height = item.DurationAsMillis / maxValue * 100d;
        if (height < 8d)
        {
            height = 8d;
        }

        return $"height:{height.ToString("0.##", CultureInfo.InvariantCulture)}%;";
    }

    private string GetAmountBarStyle(SummaryByDaysReportItemDto item)
    {
        var maxValue = Items.Any() ? Items.Max(chartItem => chartItem.Amount) : 0;
        if (maxValue <= 0 || item.Amount <= 0)
        {
            return "height:0%;";
        }

        var height = (double)(item.Amount / maxValue * 100m);
        if (height < 8d)
        {
            height = 8d;
        }

        return $"height:{height.ToString("0.##", CultureInfo.InvariantCulture)}%;";
    }
}
