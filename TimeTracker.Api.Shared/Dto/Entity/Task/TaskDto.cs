using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Task;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Shared.Dto.Entity.Task;

public class TaskDto : IResponse
{
    public long Id { get; set; }
    
    public TaskStatus Status { get; set; }
    
    public TaskPriority Priority { get; set; }
    
    public string Title { get; set; }
    
    public string? Description { get; set; }
    
    public string? ExternalTaskId { get; set; }
    
    public DateTime? StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public bool IsArchived { get; set; }
    
    public DateTime UpdateTime { get; set; }
    
    public DateTime CreateTime { get; set; }
    
    public TaskListDto TaskList { get; set; }
    
    public UserDto User { get; set; }

    public ICollection<StoredFileDto> Attachments { get; set; } = new List<StoredFileDto>();
    
    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();

    #region Calculated

    public string FormattedId
    {
        get => $"TMV#{Id}";
    }
    
    #endregion
}
