using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
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
    }
}
