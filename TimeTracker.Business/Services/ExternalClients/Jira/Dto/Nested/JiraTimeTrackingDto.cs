using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested;

public class JiraTimeTrackingDto
{
    [JsonProperty(PropertyName = "originalEstimateSeconds")]
    public long? OriginalEstimateSeconds { get; set; }
}
