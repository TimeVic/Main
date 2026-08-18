using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

public class GetSharedClientReportRequest : IRequest<GetSharedClientReportResponse>
{
    public string Token { get; set; } = string.Empty;
}
