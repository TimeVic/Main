using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;

namespace TimeTracker.Api.Shared.Dto.Entity.Task;

public class TaskCommentDto: BaseDto
{   
    public string Comment { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public UserDto User { get; set; }
    
    public TaskDto Task { get; set; }

    public ICollection<StoredFileDto> Attachments { get; set; } = new List<StoredFileDto>();
    
    public ICollection<UserDto> Watchers { get; set; } = new List<UserDto>();
}
