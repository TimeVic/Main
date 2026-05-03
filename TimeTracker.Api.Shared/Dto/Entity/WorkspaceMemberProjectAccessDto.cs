namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceMemberProjectAccessDto
{
    public virtual decimal? HourlyRate { get; set; }
    
    public virtual ProjectDto Project { get; set; } = null!;
}
