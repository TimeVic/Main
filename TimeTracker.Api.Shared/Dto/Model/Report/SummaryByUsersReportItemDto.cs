﻿namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByUsersReportItemDto
{
    public long UserId { get; set; }

    public TimeSpan Duration { get; set; }
}
