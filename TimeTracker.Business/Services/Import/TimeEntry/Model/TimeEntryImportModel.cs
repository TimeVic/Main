namespace TimeTracker.Business.Services.Import.TimeEntry.Model;

public class TimeEntryImportModel
{
    public string? ClientName { get; set; }

    public string? ProjectName { get; set; }

    public string? Description { get; set; }

    public string? TaskId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool? IsBillable { get; set; }

    public decimal? HourlyRate { get; set; }

    public ICollection<string> Tags { get; set; } = new List<string>();
}
