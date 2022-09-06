namespace TimeTracker.Web.Services.Validation;

public interface IReCaptchaService
{
    public string GetSiteKey();
    
    public string GetScriptUrl();

    public bool GetIsEnabled();

    public Task<string> GetReCaptchaTokenAsync();

    public event Action<bool> IsShowChanged;
}
