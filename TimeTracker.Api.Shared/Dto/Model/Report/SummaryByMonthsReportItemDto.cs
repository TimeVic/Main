﻿namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByMonthsReportItemDto
{
    public int Month { get; set; }

    public TimeSpan Duration { get; set; }
}
