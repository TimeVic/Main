namespace TimeTracker.Business.Orm.Dto.TimeEntry;

public class FilterDataDto
{
    public long? ClientId { get; set; }
    
    public long? ProjectId { get; set; }

    public string? Search { get; set; }
    
    public bool? IsBillable { get; set; }
    
    public long? MemberId { get; set; }
}
