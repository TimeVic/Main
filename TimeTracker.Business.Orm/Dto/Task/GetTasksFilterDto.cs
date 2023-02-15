namespace TimeTracker.Business.Orm.Dto.Task;

public class GetTasksFilterDto
{
    public long? AssignedUserId { get; set; }
    
    public string? SearchString { get; set; }
    
    public bool? IsArchived { get; set; }
    
    public bool? IsDone { get; set; }
}
