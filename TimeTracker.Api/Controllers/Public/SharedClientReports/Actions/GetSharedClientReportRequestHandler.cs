using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

namespace TimeTracker.Api.Controllers.Public.SharedClientReports.Actions;

public class GetSharedClientReportRequestHandler : IAsyncRequestHandler<GetSharedClientReportRequest, GetSharedClientReportResponse>
{
    private readonly ISharedClientReportService _sharedClientReportService;

    public GetSharedClientReportRequestHandler(ISharedClientReportService sharedClientReportService)
    {
        _sharedClientReportService = sharedClientReportService;
    }

    public async Task<GetSharedClientReportResponse> ExecuteAsync(GetSharedClientReportRequest request)
    {
        var report = await _sharedClientReportService.GetActiveAsync(request.Token);
        return await _sharedClientReportService.GetReportAsync(report);
    }
}
