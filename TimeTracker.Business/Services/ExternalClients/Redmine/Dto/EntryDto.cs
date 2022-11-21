using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

public class EntryDto
{
    [JsonProperty(PropertyName = "id")]
    public long Id { get; set; }
    
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }
}
