using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report.Summary.Parts;

public partial class FilterForm
{
    [Inject]
    public IState<ReportsState> _reportsState { get; set; }

    public SummaryReportFilterState _filterState => _reportsState.Value.SummaryReportFilter;

    private void OnChangeReportType(SummaryReportType? type)
    {
        if (_filterState.ReportType == type || type == null)
            return;
        Dispatcher.Dispatch(new ReportSetSummaryReportFilterAction(_filterState with
        {
            ReportType = type.Value
        }));
        LoadReport();
    }
    
    private void OnChangePeriodType(SummaryReportPeriodType? type)
    {
        if (_filterState.PeriodType == type || type == null)
            return;
        Dispatcher.Dispatch(new ReportSetSummaryReportFilterAction(_filterState with
        {
            PeriodType = type.Value
        }));
        LoadReport();
    }

    private void OnChangeDateStart(DateTime? dateStart)
    {
        if (_filterState.StartDate.ToShortDateString() == dateStart?.ToShortDateString())
            return;
        Dispatcher.Dispatch(new ReportSetSummaryReportFilterAction(_filterState with
        {
            StartDate = dateStart.Value
        }));
        LoadReport();
    }

    private void OnChangeDateEnd(DateTime? endDate)
    {
        if (_filterState.EndDate.ToShortDateString() == endDate?.ToShortDateString())
            return;
        Dispatcher.Dispatch(new ReportSetSummaryReportFilterAction(_filterState with
        {
            EndDate = endDate.Value
        }));
        LoadReport();
    }

    private void LoadReport()
    {
        Dispatcher.Dispatch(new ReportSetPaymentReportFilterAction(new PaymentReportFilterState(_reportsState.Value.SummaryReportFilter.EndDate)));
        Dispatcher.Dispatch(new ReportFetchSummaryReportAction());
        Dispatcher.Dispatch(new ReportFetchPaymentsReportAction());
    }
}
