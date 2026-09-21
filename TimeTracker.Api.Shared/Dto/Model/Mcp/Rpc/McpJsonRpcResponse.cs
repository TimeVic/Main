using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;

public class McpJsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    [JsonProperty("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public object? Id { get; set; }

    [JsonPropertyName("result")]
    [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpJsonRpcError? Error { get; set; }
}
