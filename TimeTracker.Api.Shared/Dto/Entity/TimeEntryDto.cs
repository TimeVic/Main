using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class TimeEntryDto : IResponse
{
    public long Id { get; set; }
    
    public string? Description { get; set; }
    
    public decimal? HourlyRate { get; set; }
    
    public bool IsBillable { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public ProjectDto? Project { get; set; }
}
