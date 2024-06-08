using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested.Content;

public class ContentDto
{
    [JsonProperty("content")]
    public IEnumerable<TextDto> Content { get; set; } = new List<TextDto>(){ new TextDto() };

    [JsonProperty("type")]
    public string Type { get; set; } = "paragraph";
}
