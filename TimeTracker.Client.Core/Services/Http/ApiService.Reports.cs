using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
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
            return await PostAsync<SummaryReportResponse?>(ApiUrl.ReportSummaryPersonal, new SummaryReportRequest()
            {
                StartTime = startDate,
                EndTime = endTime,
                Type = reportType
            });
        }

        public async Task<TeamSummaryReportResponse?> ReportsGetTeamSummaryReportAsync(
            Guid workspaceId,
            DateTime startDate,
            DateTime endTime
        )
        {
            return await PostAsync<TeamSummaryReportResponse?>(ApiUrl.ReportSummaryTeam, new TeamSummaryReportRequest
            {
                StartTime = startDate,
                EndTime = endTime
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

        public async Task<ClientShareReportSettingsResponse?> ReportsSetClientShareSettingsAsync(
            Guid clientId,
            ClientShareReportSettingsRequest request
        )
        {
            return await PostAsync<ClientShareReportSettingsResponse?>(
                $"{ApiUrl.ReportClientShareSettings}/{clientId}",
                request
            );
        }

        public async Task<GetSharedClientReportResponse?> ReportsGetPublicSharedClientReportAsync(string token)
        {
            return await GetAsync<GetSharedClientReportResponse?>(
                $"{ApiUrl.PublicSharedClientReport}/{Uri.EscapeDataString(token)}"
            );
        }

        public async Task<GetSharedClientReportTasksResponse?> ReportsGetPublicSharedClientReportTasksAsync(string token)
        {
            return await GetAsync<GetSharedClientReportTasksResponse?>(
                $"{ApiUrl.PublicSharedClientReport}/{Uri.EscapeDataString(token)}/get-tasks"
            );
        }
    }
}
