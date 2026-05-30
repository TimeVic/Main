using Newtonsoft.Json;

namespace TimeTracker.Business.Services.Auth.SocialLogin.Dto;

public class FacebookUserInfoDto
{
    [JsonProperty("id")]
    public required string Id { get; set; }
    
    [JsonProperty("verified_email")]
    public required string IsEmailVerified { get; set; }
    
    [JsonProperty("email")]
    public required string Email { get; set; }
    
    [JsonProperty("name")]
    public required string Name { get; set; }
    
    [JsonProperty("given_name")]
    public required string GivenName { get; set; }
    
    [JsonProperty("family_name")]
    public required string FamilyName { get; set; }
    
    [JsonProperty("picture")]
    public string? Picture { get; set; }
}
