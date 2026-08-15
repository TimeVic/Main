using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;

namespace TimeTracker.Client.Core.Store.Report;

public record struct ReportFetchMemberPaymentsReportAction();

public record struct ReportSetMemberPaymentReportItemsAction(ICollection<MemberPaymentsReportItemDto> Items);

public record struct ReportFetchSummaryReportAction();

public record struct ReportSetSummaryReportItemsAction(SummaryReportResponse ReportData);

public record struct ReportSetSummaryReportFilterAction(SummaryReportFilterState FilterState);

public record struct ReportResetSummaryReportFilterAction();

public record struct ReportSetMemberPaymentReportFilterAction(MemberPaymentReportFilterState FilterState);

public record struct ReportSetIsLoadingAction(bool IsLoading);

public record struct ReportFetchWorkspaceFinancialSummaryAction();

public record struct ReportSetWorkspaceFinancialSummaryAction(WorkspaceFinancialSummaryReportResponse ReportData);

public record struct ReportFetchUserPaymentReportAction();

public record struct ReportSetUserPaymentReportAction(UserPaymentReportResponse ReportData);

public record struct ReportSetUserPaymentReportFilterAction(UserPaymentReportFilterState FilterState);

public record struct ReportSetWorkspaceFinancialSummaryFilterAction(WorkspaceFinancialSummaryFilterState FilterState);

public record struct ReportResetWorkspaceFinancialSummaryFilterAction();
