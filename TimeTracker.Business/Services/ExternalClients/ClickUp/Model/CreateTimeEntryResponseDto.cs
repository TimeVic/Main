using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

public class CreateTimeEntryResponseDto
{
    [JsonProperty(PropertyName = "data")]
    public virtual TimeEntryDto? Data { get; set; }
    
    [JsonProperty(PropertyName = "err")]
    public string? Error { get; set; }
    
    [JsonProperty(PropertyName = "ECODE")]
    public string? ErrorCode { get; set; }

    public bool IsError => !string.IsNullOrEmpty(Error);
}
