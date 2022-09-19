using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

namespace TimeTracker.Web.Store.Report;

public record struct ReportFetchPaymentsReportAction();

public record struct ReportSetPaymentReportItemsAction(ICollection<PaymentsReportItemDto> Items);

public record struct ReportSetIsLoadingAction(bool IsLoading);
