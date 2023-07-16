using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested.Content;

public class DocumentDto
{
    [JsonProperty("content")]
    public IEnumerable<ContentDto> Content { get; set; } = new []{ new ContentDto() };

    [JsonProperty("type")]
    public string Type { get; set; } = "doc";
    
    [JsonProperty("version")]
    public int Version { get; set; } = 1;
    
    public void SetText(string? text)
    {
        Content.First().Content.First().Text = text ?? "";
    }
    
    public string? GetText()
    {
        return Content.FirstOrDefault()
            ?.Content
            .FirstOrDefault(item => !string.IsNullOrEmpty(item.Text))?.Text;
    }
}
