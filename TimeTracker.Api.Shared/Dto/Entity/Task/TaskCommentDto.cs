using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity.Task;

public class TaskCommentDto : IResponse
{
    public long Id { get; set; }
    
    public string Comment { get; set; }
    
    public DateTime UpdateTime { get; set; }
    
    public DateTime CreateTime { get; set; }
    
    public UserDto User { get; set; }
    
    public TaskDto Task { get; set; }

    public ICollection<StoredFileDto> Attachments { get; set; } = new List<StoredFileDto>();
    
    public ICollection<UserDto> Watchers { get; set; } = new List<UserDto>();
}
