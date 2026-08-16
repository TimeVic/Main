using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Constants;

namespace TimeTracker.Client.Core.Store.Report;

[FeatureState]
public record ReportsState
{
    public SummaryReportResponse? SummaryReportData { get; set; }

    public TeamSummaryReportResponse? TeamSummaryReportData { get; set; }

    public WorkspaceFinancialSummaryReportResponse? WorkspaceFinancialSummaryData { get; set; }

    public UserPaymentReportResponse? UserPaymentReportData { get; set; }

    public UserPaymentReportFilterState UserPaymentReportFilter { get; set; } = new(DateTime.Now);

    public bool IsLoading { get; set; }

    public SummaryReportFilterState SummaryReportFilter { get; set; } = new(
        SummaryReportType.GroupByProject,
        SummaryReportPeriodType.ThisWeek,
        DateTime.Now,
        DateTime.Now
    );
    
    public WorkspaceFinancialSummaryFilterState WorkspaceFinancialSummaryFilter { get; set; } = new(
        SummaryReportPeriodType.ThisMonth,
        DateTime.Now.StartOfMonth(),
        DateTime.Now
    );
}

public record SummaryReportFilterState(
    SummaryReportType ReportType,
    SummaryReportPeriodType PeriodType,
    DateTime StartDate,
    DateTime EndDate
);

public record UserPaymentReportFilterState(
    DateTime EndDate
);

public record WorkspaceFinancialSummaryFilterState(
    SummaryReportPeriodType PeriodType,
    DateTime StartDate,
    DateTime EndDate
);
