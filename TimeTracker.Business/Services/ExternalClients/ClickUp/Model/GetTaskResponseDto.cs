using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

public struct GetTaskResponseDto
{
    [JsonProperty(PropertyName = "id")]
    public string? Id { get; set; }
    
    [JsonProperty(PropertyName = "custom_id")]
    public string? CustomId { get; set; }
    
    [JsonProperty(PropertyName = "name")]
    public string? Name { get; set; }
    
    [JsonProperty(PropertyName = "text_content")]
    public string? TextContent { get; set; }
    
    [JsonProperty(PropertyName = "description")]
    public string? Description { get; set; }
    
    [JsonProperty(PropertyName = "date_created")]
    public string? DateCreated { get; set; }
    
    [JsonProperty(PropertyName = "date_updated")]
    public string? DateUpdated { get; set; }
    
    [JsonProperty(PropertyName = "date_closed")]
    public string? DateClosed { get; set; }
    
    [JsonProperty(PropertyName = "err")]
    public string? Error { get; set; }
    
    [JsonProperty(PropertyName = "ECODE")]
    public string? ErrorCode { get; set; }

    public bool IsError => !string.IsNullOrEmpty(Error);
}
