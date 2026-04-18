using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

public class TimeEntryResponseDto: BaseResponseDto
{
    [JsonProperty(PropertyName = "id")]
    public long Id { get; set; }
    
    [JsonProperty(PropertyName = "project")]
    public EntryDto Project { get; set; } = null!;
    
    [JsonProperty(PropertyName = "issue")]
    public EntryDto Issue { get; set; } = null!;
    
    [JsonProperty(PropertyName = "user")]
    public EntryDto User { get; set; } = null!;
    
    [JsonProperty(PropertyName = "activity")]
    public EntryDto Activity { get; set; } = null!;
    
    [JsonProperty(PropertyName = "hours")]
    public decimal Hours { get; set; }
    
    [JsonProperty(PropertyName = "comments")]
    public string Comments { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "spent_on")]
    public string SpentOn { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "created_on")]
    public DateTime CreatedOn { get; set; }
    
    [JsonProperty(PropertyName = "updated_on")]
    public DateTime UpdatedOn { get; set; }
}
