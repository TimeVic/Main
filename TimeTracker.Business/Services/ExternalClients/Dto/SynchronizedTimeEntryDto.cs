namespace TimeTracker.Business.Services.ExternalClients.Dto;

public class SynchronizedTimeEntryDto
{
    public string Id { get; set; }
    
    public string? Comment { get; set; }
    
    public string? AdditionalDescription { get; set; }

    public bool IsError { get; set; } = false;
}
