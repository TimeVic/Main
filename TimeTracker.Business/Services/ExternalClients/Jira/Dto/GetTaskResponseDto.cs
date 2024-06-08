using Newtonsoft.Json;
using TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto;

public class GetTaskResponseDto: BaseResponseDto
{
    [JsonProperty(PropertyName = "id")]
    public string? Id { get; set; }
    
    [JsonProperty(PropertyName = "key")]
    public string? Key { get; set; }

    [JsonProperty(PropertyName = "fields")]
    public TaskFieldsDto Fields { get; set; } = new();
    
    [JsonProperty(PropertyName = "renderedFields")]
    public TaskRenderedFieldsDto RenderedFields { get; set; } = new();
}
