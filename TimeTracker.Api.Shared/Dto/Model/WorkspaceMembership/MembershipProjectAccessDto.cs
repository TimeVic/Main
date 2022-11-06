using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.WorkspaceMembership;

public class MembershipProjectAccessDto
{
    public ProjectDto Project { get; set; }

    public decimal? HourlyRate { get; set; } = null;
}
