namespace TimeTracker.Business.Services.ExternalClients.Dto;

public class SynchronizedTimeEntryDto
{
    public string Id { get; set; }
    
    public string? Comment { get; set; }
    
    public string? Description { get; set; }

    public bool IsError { get; set; } = false;
}
