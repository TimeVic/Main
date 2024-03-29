﻿namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByUsersReportItemDto
{
    public long UserId { get; set; }
    
    public string UserName { get; set; }
    
    public string Email { get; set; }

    public decimal Amount { get; set; }
    
    public TimeSpan Duration { get; set; }

    public string Name
    {
        get => string.IsNullOrEmpty(UserName) ? Email : UserName;
    }
}
