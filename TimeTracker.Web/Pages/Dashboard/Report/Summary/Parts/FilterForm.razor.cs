using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Pages.Dashboard.Report.Summary.Parts;

public partial class FilterForm
{
    [Inject]
    public IState<ReportsState> _reportsState { get; set; }

    public SummaryReportFilterState _filterState => _reportsState.Value.SummaryReportFilter;
    
    private void OnChangeReportType(SummaryReportType type)
    {
        Dispatcher.Dispatch(new ReportSetSummaryReportFilterAction(_filterState with
        {
            ReportType = type
        }));
        ReloadReport();
    }
    
    private void OnChangePeriodType(SummaryReportPeriodType type)
    {
        Dispatcher.Dispatch(new ReportSetSummaryReportFilterAction(_filterState with
        {
            PeriodType = type
        }));
        ReloadReport();
    }

    private void OnChangeDateStart(DateTime? dateStart)
    {
        Dispatcher.Dispatch(new ReportSetSummaryReportFilterAction(_filterState with
        {
            StartDate = dateStart.Value
        }));
        ReloadReport();
    }

    private void OnChangeDateEnd(DateTime? endDate)
    {
        Dispatcher.Dispatch(new ReportSetSummaryReportFilterAction(_filterState with
        {
            EndDate = endDate.Value
        }));
        ReloadReport();
    }

    private void ReloadReport()
    {
        Dispatcher.Dispatch(new ReportFetchSummaryReportAction());
    }
}
