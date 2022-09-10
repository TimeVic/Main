namespace TimeTracker.Business.Orm.Dto.TimeEntry;

public class TimeEntryCreationDto
{
    public long? Id { get; set; }
    
    public string Description { get; set; }
    
    public decimal? HourlyRate { get; set; }
    
    public bool IsBillable { get; set; }
    
    public virtual TimeSpan StartTime { get; set; }
    
    public virtual TimeSpan? EndTime { get; set; }
    
    public virtual DateTime Date { get; set; }
}
