﻿using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;

namespace TimeTracker.Web.Store.Report;

public record struct ReportFetchPaymentsReportAction(
    DateTime StartTime,
    DateTime EndTime,
    SummaryReportType ReportType
);

public record struct ReportSetPaymentReportItemsAction(ICollection<PaymentsReportItemDto> Items);

public record struct ReportFetchSummaryReportAction();

public record struct ReportSetSummaryReportItemsAction(SummaryReportResponse ReportData);

public record struct ReportSetIsLoadingAction(bool IsLoading);
