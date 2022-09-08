using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceDto : IResponse
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public bool IsDefault { get; set; }
}
