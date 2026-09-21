using System.Text.Json.Serialization;

namespace TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;

public class McpToolContentDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

