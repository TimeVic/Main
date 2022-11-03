using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Web.Constants;

namespace TimeTracker.Web.Store.Report;

[FeatureState]
public record ReportsState
{
    public ICollection<PaymentsReportItemDto> PaymentReportItems { get; set; } = new List<PaymentsReportItemDto>();
    
    public SummaryReportResponse? SummaryReportData { get; set; }

    public bool IsLoading { get; set; }

    public SummaryReportFilterState SummaryReportFilter { get; set; } = new(
        SummaryReportType.GroupByProject,
        SummaryReportPeriodType.ThisWeek,
        DateTime.Now.AddDays(-7),
        DateTime.Now
    );
    
    public PaymentReportFilterState PaymentReportFilter { get; set; } = new(
        DateTime.Now
    );
}

public record SummaryReportFilterState(
    SummaryReportType ReportType,
    SummaryReportPeriodType PeriodType,
    DateTime StartDate,
    DateTime EndDate
);

public record PaymentReportFilterState(
    DateTime EndDate
);
