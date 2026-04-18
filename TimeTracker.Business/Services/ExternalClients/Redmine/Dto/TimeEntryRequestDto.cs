using Newtonsoft.Json;

namespace TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

public class TimeEntryRequestDto
{
    [JsonProperty(PropertyName = "issue_id")]
    public string TaskId { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "spent_on")]
    public string SpentOn { get; private set;  } = string.Empty;

    [JsonProperty(PropertyName = "hours")]
    public double Hours { get; private set;  }
    
    [JsonProperty(PropertyName = "comments")]
    public string Comments { get; set;  } = string.Empty;
    
    [JsonProperty(PropertyName = "user_id")]
    public long UserId { get; set;  }
    
    [JsonProperty(PropertyName = "activity_id")]
    public long ActivityId { get; set;  }
    
    [JsonIgnore]
    public DateOnly SpentOnDate
    {
        set
        {
            SpentOn = value.ToString("yyyy-MM-dd");
        }
    }
    
    [JsonIgnore]
    public TimeSpan Duration
    {
        set
        {
            Hours = value.TotalHours;
        }
    }
}
