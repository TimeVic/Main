using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

public class GetSharedClientReportTasksRequest : IRequest<GetSharedClientReportTasksResponse>
{
    public string Token { get; set; } = string.Empty;
}
