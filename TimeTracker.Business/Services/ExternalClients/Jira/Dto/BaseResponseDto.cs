using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Jira.Dto;

public class BaseResponseDto
{
    [JsonProperty("errors")]
    public IDictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();

    [JsonProperty("errorMessages")]
    public IEnumerable<string> ErrorMessages { get; set; } = new List<string>();
    
    public bool IsError => Errors.Count() != 0 || ErrorMessages.Count() != 0;
}
