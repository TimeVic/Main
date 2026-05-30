using Newtonsoft.Json;

namespace TimeTracker.Business.Services.Auth.SocialLogin.Dto;

public class OauthLoginDto
{
    [JsonProperty("access_token")]
    public required string AccessToken { get; set; }
    
    [JsonProperty("id_token")]
    public string? IdToken { get; set; }
    
    [JsonProperty("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [JsonProperty("scope")]
    public string? Scope { get; set; }
}
