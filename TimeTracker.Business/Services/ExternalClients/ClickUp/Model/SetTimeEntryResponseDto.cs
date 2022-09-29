using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

public struct SetTimeEntryResponseDto
{
    [JsonProperty(PropertyName = "id")]
    public long? Id { get; set; }
    
    [JsonProperty(PropertyName = "err")]
    public string? Error { get; set; }
    
    [JsonProperty(PropertyName = "ECODE")]
    public string? ErrorCode { get; set; }

    public bool IsError => !string.IsNullOrEmpty(Error);
}
