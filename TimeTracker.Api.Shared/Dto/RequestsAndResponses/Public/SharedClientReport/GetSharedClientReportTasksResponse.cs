using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

public class GetSharedClientReportTasksResponse : IResponse
{
    public ICollection<SharedClientReportTaskDto> Tasks { get; set; } = new List<SharedClientReportTaskDto>();

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public bool IsHasMore { get; set; }
}
