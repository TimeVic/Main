using TimeTracker.Business.Services.Auth.SocialLogin.Dto;

namespace TimeTracker.Business.Services.Auth.SocialLogin;

public interface ISocialLoginProvider
{
    public string BuildLoginUrl(Uri? loginReturnUrl = null, Uri? registrationReturnUrl = null);
    
    public Task<UserInfoDto> HandleCallback(string code, string? state);
    
    public Task<UserInfoDto> HandleIdToken(string token);
}
