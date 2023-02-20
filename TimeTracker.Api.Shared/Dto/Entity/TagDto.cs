using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class TagDto : IResponse
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public string? Color { get; set; }
    
    public string TextColor { get; set; }
}
