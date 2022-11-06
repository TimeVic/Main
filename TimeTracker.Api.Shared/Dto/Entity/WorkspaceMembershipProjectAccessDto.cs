namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceMembershipProjectAccessDto
{
    public virtual decimal? HourlyRate { get; set; }
    
    public virtual ProjectDto Project { get; set; }
}
