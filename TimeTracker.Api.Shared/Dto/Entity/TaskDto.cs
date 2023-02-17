using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class TaskDto : IResponse
{
    public long Id { get; set; }
    
    public string Title { get; set; }
    
    public string? Description { get; set; }
    
    public string? ExternalTaskId { get; set; }
    
    public DateTime? NotificationTime { get; set; }
    
    public bool IsDone { get; set; }
    
    public bool IsArchived { get; set; }
    
    public DateTime UpdateTime { get; set; }
    
    public DateTime CreateTime { get; set; }
    
    public TaskListDto TaskList { get; set; }
    
    public UserDto User { get; set; }

    public ICollection<StoredFileDto> Attachments { get; set; } = new List<StoredFileDto>();
}
