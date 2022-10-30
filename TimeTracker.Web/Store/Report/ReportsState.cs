using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

namespace TimeTracker.Web.Store.Report;

[FeatureState]
public record ReportsState
{
    public ICollection<PaymentsReportItemDto> PaymentReportItems { get; set; } = new List<PaymentsReportItemDto>();
    
    public SummaryReportResponse? SummaryReportData { get; set; }

    public bool IsLoading { get; set; }
}
