using System.Text.Json.Serialization;

namespace TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;

public class McpToolDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public object InputSchema { get; set; } = new { type = "object" };
}

