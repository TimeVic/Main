using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class TimeEntryDto: BaseDto
{   
    public string? Description { get; set; }
    
    public decimal? HourlyRate { get; set; }
    
    public bool IsBillable { get; set; }
    
    public DateOnly Date { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan? EndTime { get; set; }

    public bool IsSynced { get; set; }
    
    public ProjectDto? Project { get; set; }
    
    public UserDto User { get; set; }
    
    public TaskDto? Task { get; set; }
    
    public bool IsActive => EndTime == null;
    
    public TimeSpan Duration => EndTime == null ? TimeSpan.Zero : EndTime.Value - StartTime;
    
    public void UpdateFrom(TimeEntryDto fromEntry)
    {
        Id = fromEntry.Id;
        Description = fromEntry.Description;
        Project = fromEntry.Project;
        EndTime = fromEntry.EndTime;
        StartTime = fromEntry.StartTime;
        HourlyRate = fromEntry.HourlyRate;
        Date = fromEntry.Date;
        IsBillable = fromEntry.IsBillable;
        Task = fromEntry.Task;
        IsSynced = fromEntry.IsSynced;
    }
}
