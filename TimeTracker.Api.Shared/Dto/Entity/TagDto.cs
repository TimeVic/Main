using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class TagDto: BaseDto
{   
    public string Name { get; set; } = string.Empty;
    
    public string? Color { get; set; }
    
    public string TextColor { get; set; } = string.Empty;
}
