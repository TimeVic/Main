using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<PaymentReportResponse> ReportsGetPaymentsReportAsync(long workspaceId)
        {
            var response = await PostAuthorizedAsync<PaymentReportResponse>(ApiUrl.ReportPayments, new PaymentReportRequest()
            {
                WorkspaceId = workspaceId
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<SummaryReportResponse> ReportsGetSummaryReportAsync(
            long workspaceId,
            DateTime startDate,
            DateTime endTime,
            SummaryReportType reportType
        )
        {
            var response = await PostAuthorizedAsync<SummaryReportResponse>(ApiUrl.ReportPayments, new SummaryReportRequest()
            {
                WorkspaceId = workspaceId,
                StartTime = startDate,
                EndTime = endTime,
                Type = reportType
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
