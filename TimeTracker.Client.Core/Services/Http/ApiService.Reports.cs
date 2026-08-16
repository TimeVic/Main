using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<SummaryReportResponse?> ReportsGetSummaryReportAsync(
            Guid workspaceId,
            DateTime startDate,
            DateTime endTime,
            SummaryReportType reportType
        )
        {
            return await PostAsync<SummaryReportResponse?>(ApiUrl.ReportSummary, new SummaryReportRequest()
            {
                StartTime = startDate,
                EndTime = endTime,
                Type = reportType
            });
        }

        public async Task<WorkspaceFinancialSummaryReportResponse?> ReportsGetWorkspaceFinancialSummaryAsync(Guid workspaceId)
        {
            return await PostAsync<WorkspaceFinancialSummaryReportResponse?>(
                ApiUrl.ReportWorkspaceFinancialSummary,
                new WorkspaceFinancialSummaryReportRequest
                {
                }
            );
        }

        public async Task<UserPaymentReportResponse?> ReportsGetUserPaymentReportAsync(Guid workspaceId, DateTime endDate)
        {
            return await PostAsync<UserPaymentReportResponse?>(
                ApiUrl.ReportUserPayment,
                new UserPaymentReportRequest
                {
                    EndDate = endDate
                }
            );
        }
    }
}
