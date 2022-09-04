namespace TimeTracker.Business.Orm.Dto.TimeEntry;

public class TimeEntryCreationDto
{
    public long Id { get; set; }
    
    public string Description { get; set; }
    
    public decimal? HourlyRate { get; set; }
    
    public bool IsBillable { get; set; }
    
    public virtual DateTime StartTime { get; set; }
    
    public virtual DateTime EndTime { get; set; }
}
