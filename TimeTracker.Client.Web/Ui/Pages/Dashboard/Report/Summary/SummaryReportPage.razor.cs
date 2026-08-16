using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Store.Report;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.Summary;

public partial class SummaryReportPage
{
    [Inject]
    public IState<ReportsState> _state { get; set; }

    public bool _isLoaded => _state.Value.SummaryReportData != null;

    public IEnumerable<SummaryByDaysReportItemDto> _byDateItems
    {
        get => FillReportSkippedDays(_state.Value.SummaryReportData?.ByDays ?? new List<SummaryByDaysReportItemDto>());
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ReportResetSummaryReportFilterAction());
        Dispatcher.Dispatch(new ReportFetchSummaryReportAction());
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
