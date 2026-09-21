using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;

public class McpJsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    [JsonProperty("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public object? Id { get; set; }

    [JsonPropertyName("method")]
    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    [JsonProperty("params")]
    public object? Params { get; set; }
}

