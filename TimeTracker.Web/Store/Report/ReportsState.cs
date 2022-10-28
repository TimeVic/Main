using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;

namespace TimeTracker.Web.Store.Report;

[FeatureState]
public record ReportsState
{
    public ICollection<PaymentsReportItemDto> PaymentReportItems { get; set; } = new List<PaymentsReportItemDto>();

    public bool IsLoading { get; set; }
}
