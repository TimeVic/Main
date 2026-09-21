using System.Text.Json.Serialization;

namespace TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;

public class McpServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "TimeVic";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}

