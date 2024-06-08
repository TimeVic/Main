using System.Text.Json.Serialization;
using Newtonsoft.Json;
using TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested.Content;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto;

public class SetTimeEntryDto
{
    [JsonProperty("comment")]
    public DocumentDto Comment { get; set; } = new();
    
    [JsonProperty("started")]
    public string Started { get; set; }

    [JsonProperty("timeSpentSeconds")]
    public long TimeSpentSeconds { get; set; }
}
