namespace TimeTracker.Business.Services.Import.TimeEntry.Model;

public class TimeEntryImportResultDto
{
    public int ImportedCount { get; set; }

    public int SkippedCount { get; set; }

    public int TotalCount { get; set; }
}
