using System.Text.Json.Serialization;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

public struct TimeEntryDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("wid")]
    public long WorkspaceId { get; set; }

    [JsonPropertyName("start")]
    public long Start { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("end")]
    public long End { get; set; }
    
    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}
