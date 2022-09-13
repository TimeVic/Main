using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class ProjectDto : IResponse
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public bool IsBillableByDefault { get; set; }
    
    public decimal? DefaultHourlyRate { get; set; }
    
    public bool IsArchived { get; set; }
    
    public ClientDto Client { get; set; }
}
