using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

namespace TimeTracker.Api.Controllers.Public.SharedClientReports.Actions;

public class GetSharedClientReportTasksRequestHandler : IAsyncRequestHandler<GetSharedClientReportTasksRequest, GetSharedClientReportTasksResponse>
{
    private readonly ISharedClientReportService _sharedClientReportService;

    public GetSharedClientReportTasksRequestHandler(ISharedClientReportService sharedClientReportService)
    {
        _sharedClientReportService = sharedClientReportService;
    }

    public async Task<GetSharedClientReportTasksResponse> ExecuteAsync(GetSharedClientReportTasksRequest request)
    {
        var report = await _sharedClientReportService.GetActiveAsync(request.Token, isRequireTasks: true);
        return await _sharedClientReportService.GetTasksAsync(report, request.ProjectId, request.Page);
    }
}
