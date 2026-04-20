using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report.Summary;

public partial class SummaryReportPage
{
    [Inject]
    public IState<ReportsState> _state { get; set; }

    public bool _isLoaded => _state.Value.SummaryReportData != null;

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    public IEnumerable<SummaryByDaysReportItemDto> _byDateItems
    {
        get => FillReportSkippedDays(_state.Value.SummaryReportData?.ByDays ?? new List<SummaryByDaysReportItemDto>());
    }
    
    public bool _isShowChartWithLineSeries
    {
        get
        {
            var firstItem = _byDateItems.FirstOrDefault();
            var lastItem = _byDateItems.LastOrDefault();
            if (firstItem == null || lastItem == null)
            {
                return true;
            }

            if (firstItem.Date - lastItem.Date > TimeSpan.FromDays(4))
            {
                return true;
            }

            return false;
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ReportResetSummaryReportFilterAction());
        Dispatcher.Dispatch(new ReportSetPaymentReportFilterAction(new PaymentReportFilterState(_state.Value.SummaryReportFilter.EndDate)));
        Dispatcher.Dispatch(new ReportFetchSummaryReportAction());
        Dispatcher.Dispatch(new ReportFetchPaymentsReportAction());
    }
    
    private string FormatterDuration(object durationObject)
    {
        var duration = TimeSpan.FromMilliseconds((double)durationObject);
        return duration.ToReadableShortString();
    }
    
    private string GetDurationBarStyle(SummaryByDaysReportItemDto item)
    {
        var maxValue = _byDateItems.Max(chartItem => chartItem.DurationAsMillis);
        if (maxValue <= 0 || item.DurationAsMillis <= 0)
        {
            return "height:0%;";
        }

        var height = item.DurationAsMillis / maxValue * 100d;
        if (height < 8d)
        {
            height = 8d;
        }

        return $"height:{height:0.##}%;";
    }

    private string GetAmountBarStyle(SummaryByDaysReportItemDto item)
    {
        var maxValue = _byDateItems.Max(chartItem => chartItem.Amount);
        if (maxValue <= 0 || item.Amount <= 0)
        {
            return "height:0%;";
        }

        var height = (double)(item.Amount / maxValue * 100m);
        if (height < 8d)
        {
            height = 8d;
        }

        return $"height:{height:0.##}%;";
    }
    
    private ICollection<SummaryByDaysReportItemDto> FillReportSkippedDays(ICollection<SummaryByDaysReportItemDto> items)
    {
        items = items.OrderBy(item => item.Date).ToList();
        if (items.Count == 0)
        {
            return items;
        }
        if (
            _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.Custom
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.LastMonth
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.ThisMonth
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.Past2Weeks
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.ThisWeek
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.Today
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.Yesterday
        )
        {
            return items;
        }
        
        var result = new List<SummaryByDaysReportItemDto>();
        var currentDate = items.First().Date;
        var lastDate = items.Last().Date;
        var itemsDictionary = items.OrderBy(item => item.Date).ToDictionary(item => item.Date);
        while (currentDate <= lastDate)
        {
            if (itemsDictionary.TryGetValue(currentDate, out var item))
            {
                result.Add(item);
            }
            else
            {
                result.Add(new SummaryByDaysReportItemDto
                {
                    Date = currentDate,
                    Duration = TimeSpan.Zero,
                    Amount = 0m
                });
            }

            currentDate = currentDate.AddDays(1);
        }
        return result;
    }
}
