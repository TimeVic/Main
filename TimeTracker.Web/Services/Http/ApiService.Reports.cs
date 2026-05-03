using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<MemberPaymentReportResponse?> ReportsGetMemberPaymentsReportAsync(Guid workspaceId, DateTime endDate)
        {
            return await PostAsync<MemberPaymentReportResponse?>(ApiUrl.ReportMemberPayments, new MemberPaymentReportRequest()
            {
                WorkspaceId = workspaceId,
                EndDate = endDate
            });
        }
        
        public async Task<SummaryReportResponse?> ReportsGetSummaryReportAsync(
            Guid workspaceId,
            DateTime startDate,
            DateTime endTime,
            SummaryReportType reportType
        )
        {
            return await PostAsync<SummaryReportResponse?>(ApiUrl.ReportSummary, new SummaryReportRequest()
            {
                WorkspaceId = workspaceId,
                StartTime = startDate,
                EndTime = endTime,
                Type = reportType
            });
        }

        public async Task<WorkspaceFinancialSummaryReportResponse?> ReportsGetWorkspaceFinancialSummaryAsync(
            Guid workspaceId,
            DateTime startDate,
            DateTime endDate
        )
        {
            return await PostAsync<WorkspaceFinancialSummaryReportResponse?>(
                ApiUrl.ReportWorkspaceFinancialSummary,
                new WorkspaceFinancialSummaryReportRequest
                {
                    WorkspaceId = workspaceId,
                    StartDate = startDate,
                    EndDate = endDate
                }
            );
        }
    }
}
