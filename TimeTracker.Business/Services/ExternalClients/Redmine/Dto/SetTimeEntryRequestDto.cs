using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

public class SetTimeEntryRequestDto
{
    [JsonProperty(PropertyName = "time_entry")]
    public TimeEntryRequestDto TimeEntryRequest { get; set; }
}
