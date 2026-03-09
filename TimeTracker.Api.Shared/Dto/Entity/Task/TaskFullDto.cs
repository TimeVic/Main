using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Task;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Shared.Dto.Entity.Task;

public class TaskFullDto : TaskDto
{
    public UserDto User { get; set; }

    public ICollection<StoredFileDto> Attachments { get; set; } = new List<StoredFileDto>();
    
    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();

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
