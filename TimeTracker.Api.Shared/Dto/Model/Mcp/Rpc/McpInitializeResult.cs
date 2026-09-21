using System.Text.Json.Serialization;

namespace TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;

public class McpInitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public object Capabilities { get; set; } = new { tools = new { } };

    [JsonPropertyName("serverInfo")]
    public McpServerInfo ServerInfo { get; set; } = new();
}

