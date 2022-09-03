namespace TimeTracker.Api.Shared.Dto.Entity;

public class UserDto
{
    public long Id { get; set; }
    
    public virtual string? UserName { get; set; }
    
    public virtual string Email { get; set; }
}
