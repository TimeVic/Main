using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Shared.Dto.Entity.Task;

public class TaskDto : IResponse
{
    public Guid Id { get; set; }
    
    public int PositionIndex { get; set; }
    
    public long TaskId { get; set; }
    
    public TaskStatus Status { get; set; }
    
    public TaskPriority Priority { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? ExternalTaskId { get; set; }

    public TimeSpan? OriginalEstimate { get; set; }

    public ExternalSourceType ExternalSourceType { get; set; }

    public TimeSpan TrackedDuration { get; set; }
    
    public DateTime? StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public DateTime? ReminderTime { get; set; }
    
    public bool IsArchived { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public TaskListDto TaskList { get; set; } = null!;
    
    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();
    
    public UserDto User { get; set; } = null!;
    
    #region Calculated

    public string FormattedId
    {
        get => string.IsNullOrEmpty(ExternalTaskId) ? $"#{TaskId}" : ExternalTaskId;
    }

    public DateTime? CalculatedStartTime
    {
        get
        {
            if (StartTime == null && EndTime.HasValue)
            {
                return EndTime.Value.AddHours(-1);
            }

            return StartTime;
        }
    }
    
    public DateTime? CalculatedEndTime
    {
        get
        {
            if (StartTime.HasValue && EndTime == null)
            {
                return StartTime.Value.AddHours(1);
            }
            return EndTime;
        }
    }
    
    public DateTime? DueTime
    {
        get
        {
            if (EndTime.HasValue)
                return EndTime.Value;
            return StartTime;
        }
    }
    
    #endregion
}
