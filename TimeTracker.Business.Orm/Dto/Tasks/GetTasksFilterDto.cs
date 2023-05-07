namespace TimeTracker.Business.Orm.Dto.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

public class GetTasksFilterDto
{
    public long? AssignedUserId { get; set; }
    
    public string? SearchString { get; set; }
    
    public TaskStatus? Status { get; set; }
    
    public ICollection<TaskStatus>? Statuses { get; set; }
    
    public bool? IsArchived { get; set; }
}
