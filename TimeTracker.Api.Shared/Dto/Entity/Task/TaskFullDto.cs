using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Task;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Shared.Dto.Entity.Task;

public class TaskFullDto : TaskDto
{
    public ICollection<StoredFileDto> Attachments { get; set; } = new List<StoredFileDto>();
    
    public ICollection<TaskSubTaskDto> SubTasks { get; set; } = new List<TaskSubTaskDto>();

    #region Calculated

    public new string FormattedId
    {
        get => string.IsNullOrEmpty(ExternalTaskId) ? $"#{TaskId}" : ExternalTaskId;
    }

    public new DateTime? CalculatedStartTime
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
    
    public new DateTime? CalculatedEndTime
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
    
    public new DateTime? DueTime
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
