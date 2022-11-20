using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

public class TimeEntryResponseDto: BaseResponseDto
{
    [JsonProperty(PropertyName = "id")]
    public long Id { get; set; }
    
    [JsonProperty(PropertyName = "project")]
    public EntryDto Project { get; set; }
    
    [JsonProperty(PropertyName = "issue")]
    public EntryDto Issue { get; set; }
    
    [JsonProperty(PropertyName = "user")]
    public EntryDto User { get; set; }
    
    [JsonProperty(PropertyName = "activity")]
    public EntryDto Activity { get; set; }
    
    [JsonProperty(PropertyName = "hours")]
    public decimal Hours { get; set; }
    
    [JsonProperty(PropertyName = "comments")]
    public string Comments { get; set; }
    
    [JsonProperty(PropertyName = "spent_on")]
    public string SpentOn { get; set; }
    
    [JsonProperty(PropertyName = "created_on")]
    public DateTime CreatedOn { get; set; }
    
    [JsonProperty(PropertyName = "updated_on")]
    public DateTime UpdatedOn { get; set; }
}
