namespace TimeTracker.Business.Services.Auth.SocialLogin.Dto;

public class AppleUserInfoDto
{
    public required string Id { get; set; }
    
    public required string Email { get; set; }
    
    public required string Name { get; set; }
    
    public string GivenName { get; set; } = string.Empty;
    
    public string FamilyName { get; set; } = string.Empty;
 }
