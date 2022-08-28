using Domain.Abstractions;

namespace TimeTracker.Business.Common.Services.Web.ReCaptcha;

public interface IReCaptchaService: IDomainService
{
    Task<bool> ValidateAsync(string token);
    
    bool HasSecretKey();
}
