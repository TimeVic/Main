using System.Text.Json.Serialization;
using Newtonsoft.Json;
using TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested.Content;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto;

public class TimeEntryDto: BaseResponseDto
{
    [JsonProperty("id")]
    public long Id { get; set; }
    
    [JsonProperty("issueId")]
    public long IssueId { get; set; }
    
    [JsonProperty("comment")]
    public DocumentDto Comment { get; set; } = new();
}
