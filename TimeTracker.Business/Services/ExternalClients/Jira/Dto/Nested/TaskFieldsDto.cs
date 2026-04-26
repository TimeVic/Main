using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested;

public class TaskFieldsDto
{
    [JsonProperty(PropertyName = "summary")]
    public string? Summary { get; set; }

    [JsonProperty(PropertyName = "timetracking")]
    public JiraTimeTrackingDto? Timetracking { get; set; }
    
    [JsonProperty(PropertyName = "created")]
    public DateTime Created { get; set; }
}
