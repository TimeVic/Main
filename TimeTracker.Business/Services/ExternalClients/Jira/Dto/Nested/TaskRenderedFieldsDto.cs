using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested;

public class TaskRenderedFieldsDto
{
    [JsonProperty(PropertyName = "description")]
    public string? DescriptionHtml { get; set; }
}
