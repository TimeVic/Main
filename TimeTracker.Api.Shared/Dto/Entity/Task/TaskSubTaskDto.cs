using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity.Task;

public class TaskSubTaskDto : IResponse
{
    public Guid Id { get; set; }
    
    public Guid TaskId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
    
    public int PositionIndex { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}
