using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class TaskListDto : IResponse
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public ProjectDto Project { get; set; }
}
