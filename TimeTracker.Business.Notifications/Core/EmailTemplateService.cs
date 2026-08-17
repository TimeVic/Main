using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Core;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly EmailFactory _emailFactory = new();
    private readonly IUserDao _userDao;
    private readonly string _frontendUrl;

    public EmailTemplateService(IUserDao userDao, IConfiguration configuration)
    {
        _userDao = userDao;
        _frontendUrl = configuration.GetValue<string>("App:FrontendUrl")?.TrimEnd('/') ?? string.Empty;
    }

    public async Task<EmailBuilder> GetEmailBuilderAsync(string templateName, string recipientEmail)
    {
        var languageCode = await _userDao.GetLanguageCodeByEmailAsync(recipientEmail);
        var normalizedLanguageCode = string.Equals(languageCode, "uk-UA", StringComparison.OrdinalIgnoreCase)
            ? "uk-UA"
            : "en";
        var emailBuilder = _emailFactory.GetEmailBuilder(templateName, normalizedLanguageCode);
        emailBuilder.AddPlaceholder("host", _frontendUrl);
        emailBuilder.AddPlaceholder("faqUrl", GetLocalizedUrl("/faq", normalizedLanguageCode));
        emailBuilder.AddPlaceholder("privacyUrl", GetLocalizedUrl("/privacy-policy", normalizedLanguageCode));
        return emailBuilder;
    }

    private string GetLocalizedUrl(string path, string languageCode)
    {
        return languageCode == "uk-UA" ? $"{_frontendUrl}/uk{path}" : $"{_frontendUrl}{path}";
    }
}
