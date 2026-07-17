namespace TimeTracker.Business.Orm.Dto.TimeEntry;

public class FilterDataDto
{
    public Guid? ClientId { get; set; }
    
    public Guid? ProjectId { get; set; }

    public Guid? TaskId { get; set; }

    public string? Search { get; set; }
    
    public bool? IsBillable { get; set; }
    
    public Guid? MemberId { get; set; }
    
    public DateTime? DateFrom { get; set; }
    
    public DateTime? DateTo { get; set; }
}
