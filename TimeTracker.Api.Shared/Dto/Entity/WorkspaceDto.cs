using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceDto : IResponse
{
    public long Id { get; set; }
    
    public virtual string Name { get; set; }
}
