using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested.Content;

public class TextDto
{
    [JsonProperty("type")]
    public string Type { get; set; } = "text";
    
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;
}
