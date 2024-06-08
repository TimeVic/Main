using System.Text.Json.Serialization;
using Newtonsoft.Json;
using TimeTracker.Business.Services.ExternalClients.Jira.Dto.Nested.Content;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto;

public class CheckSettingsResponseDto: BaseResponseDto
{
    [JsonProperty("total")]
    public long Total { get; set; }
    
    public bool IsCorrect
    {
        get => Total > 0;
    }
}
