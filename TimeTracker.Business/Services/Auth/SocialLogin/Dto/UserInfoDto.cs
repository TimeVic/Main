namespace TimeTracker.Business.Services.Auth.SocialLogin.Dto;

public class UserInfoDto
{
    public string? GoogleId { get; set; }
    public string? FacebookId { get; set; }
    public string? AppleId { get; set; }
    public required string Email { get; set; }
    public required string? FirstName { get; set; }
    public required string? LastName { get; set; }
    
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public Uri? LoginReturnUrl { get; set; }
    public Uri? RegistrationReturnUrl { get; set; }
}
