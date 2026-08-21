using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace.TimeEntry;

public class ImportResponse : IResponse
{
    public int ImportedCount { get; set; }

    public int SkippedCount { get; set; }

    public int TotalCount { get; set; }
}
