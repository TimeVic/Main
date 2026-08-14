using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;

public class WorkspaceFinancialMemberBalanceDto
{
    public Guid MemberId { get; set; }

    public UserDto User { get; set; } = null!;

    public TimeSpan Duration { get; set; }

    public decimal Cost { get; set; }

    public decimal PaidOut { get; set; }

    public decimal Owed { get; set; }

    public DateTime? LastPayoutDate { get; set; }

    public ICollection<WorkspaceFinancialMemberProjectDto> Projects { get; set; } = new List<WorkspaceFinancialMemberProjectDto>();
}
