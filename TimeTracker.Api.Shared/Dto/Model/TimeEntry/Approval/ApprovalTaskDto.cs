using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;

public class ApprovalTaskDto : IResponse
{
    public Guid? TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ExternalTaskId { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public IReadOnlyList<TimeEntryDto> Entries { get; set; } = new List<TimeEntryDto>();
}
