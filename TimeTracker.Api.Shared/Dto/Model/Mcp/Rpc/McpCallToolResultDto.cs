using System.Text.Json.Serialization;

namespace TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;

public class McpCallToolResultDto
{
    [JsonPropertyName("content")]
    public List<McpToolContentDto> Content { get; set; } = new();

    [JsonPropertyName("isError")]
    public bool IsError { get; set; }
}

