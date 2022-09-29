using System.Text.Json.Serialization;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

public struct SetTimeEntryDto
{
    [JsonPropertyName("start")]
    public long Start { get; set; }
    
    [JsonPropertyName("end")]
    public long End { get; set; }
}
