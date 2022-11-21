using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

public class SetTimeEntryResponseDto: BaseResponseDto
{
    [JsonProperty(PropertyName = "time_entry")]
    public TimeEntryResponseDto TimeEntryRequest { get; set; }
}
