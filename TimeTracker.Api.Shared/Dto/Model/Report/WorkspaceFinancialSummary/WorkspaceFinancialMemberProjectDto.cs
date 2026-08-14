using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;

public class WorkspaceFinancialMemberProjectDto
{
    public ProjectDto Project { get; set; } = null!;

    public ClientDto? Client { get; set; }

    public TimeSpan Duration { get; set; }

    public decimal Earned { get; set; }
}
