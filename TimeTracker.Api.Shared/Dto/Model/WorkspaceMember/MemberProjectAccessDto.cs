using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.WorkspaceMember;

public class MemberProjectAccessDto
{
    public ProjectDto Project { get; set; } = null!;

    public decimal? HourlyRate { get; set; } = null;
}
