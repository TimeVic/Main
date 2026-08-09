using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

public class StartResponse: IResponse
{
    public TimeEntryDto ActiveTimeEntry { get; set; } = null!;

    public TimeEntryDto? StoppedTimeEntry { get; set; }
}
