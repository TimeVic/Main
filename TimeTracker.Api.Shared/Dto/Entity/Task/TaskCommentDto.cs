using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;

namespace TimeTracker.Api.Shared.Dto.Entity.Task;

public class TaskCommentDto: BaseDto
{   
    public string Comment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public UserDto User { get; set; } = null!;
    
    public TaskDto Task { get; set; } = null!;

    public ICollection<StoredFileDto> Attachments { get; set; } = new List<StoredFileDto>();
    
    public ICollection<UserDto> Watchers { get; set; } = new List<UserDto>();
}
