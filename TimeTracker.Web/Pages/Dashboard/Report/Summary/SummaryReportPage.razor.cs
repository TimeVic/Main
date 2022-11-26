using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Pages.Dashboard.Report.Summary;

public partial class SummaryReportPage
{
    [Inject]
    public IState<ReportsState> _state { get; set; }

    public bool _isLoaded => _state.Value.SummaryReportData != null;

    public IEnumerable<SummaryByDaysReportItemDto> _byDateItems
    {
        get => _state.Value.SummaryReportData?.ByDays ?? new List<SummaryByDaysReportItemDto>();
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
        Dispatcher.Dispatch(new ReportFetchSummaryReportAction());
    }
    
    private string FormatterDuration(object durationObject)
    {
        var duration = TimeSpan.FromMilliseconds((double)durationObject);
        return duration.ToReadableShortString();
    }
}
