using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;

public class ApprovalProjectDto : IResponse
{
    public Guid? ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public decimal TotalDeveloperAmount { get; set; }
    public decimal TotalClientAmount { get; set; }
    public IReadOnlyList<ApprovalTaskDto> Tasks { get; set; } = new List<ApprovalTaskDto>();
}
