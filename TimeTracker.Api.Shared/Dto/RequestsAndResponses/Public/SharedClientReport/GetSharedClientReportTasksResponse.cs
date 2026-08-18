using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

public class GetSharedClientReportTasksResponse : IResponse
{
    public ICollection<SharedClientReportTaskDto> Tasks { get; set; } = new List<SharedClientReportTaskDto>();
}
