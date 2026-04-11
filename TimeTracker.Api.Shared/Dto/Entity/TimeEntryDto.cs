using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Common.Attributes;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class TimeEntryDto: BaseDto
{   
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public decimal? HourlyRate { get; set; }
    
    public bool IsBillable { get; set; }

    public bool IsSynced { get; set; }
    
    public ProjectDto? Project { get; set; }
    
    public UserDto User { get; set; }
    
    public TaskDto? Task { get; set; }
    
    #region Time

    [NonConvertibleDateTime]
    public DateTime StartTime
    {
        get;
        set => field = value.ToTimeZone(TimeZone);
    }

    [NonConvertibleDateTime]
    public DateTime? EndTime
    {
        get;
        set => field = value.ToTimeZone(TimeZone);
    }

    public string TimeZone { get; set; }
    
    #endregion
    
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
        IsBillable = fromEntry.IsBillable;
        Task = fromEntry.Task;
        IsSynced = fromEntry.IsSynced;
    }
}
