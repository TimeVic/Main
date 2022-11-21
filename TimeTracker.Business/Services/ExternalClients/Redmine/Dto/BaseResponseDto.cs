using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

public abstract class BaseResponseDto
{
    [JsonProperty(PropertyName = "id")]
    public ICollection<string> Errors { get; set; } = new List<string>();

    public bool IsSuccess => Errors.Count == 0;
}
