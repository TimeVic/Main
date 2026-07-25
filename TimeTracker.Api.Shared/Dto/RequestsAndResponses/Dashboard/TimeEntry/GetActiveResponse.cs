using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

public class GetActiveResponse : IResponse
{
    public TimeEntryDto? ActiveTimeEntry { get; set; }
}
