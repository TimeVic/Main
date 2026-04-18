namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByUsersReportItemDto
{
    public Guid UserId { get; set; }
    
    public string UserName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    
    public TimeSpan Duration { get; set; }

    public string Name
    {
        get => string.IsNullOrEmpty(UserName) ? Email : UserName;
    }
}
