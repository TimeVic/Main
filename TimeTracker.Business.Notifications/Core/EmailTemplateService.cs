using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Notifications.Core;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly EmailFactory _emailFactory = new();
    private readonly string _frontendUrl;

    public EmailTemplateService(IConfiguration configuration)
    {
        _frontendUrl = configuration.GetValue<string>("App:FrontendUrl")?.TrimEnd('/') ?? string.Empty;
    }

    public EmailBuilder GetEmailBuilder(string templateName, UserEntity recipient)
    {
        return GetEmailBuilder(templateName, recipient.Language.Code);
    }

    public EmailBuilder GetEmailBuilder(string templateName, string languageCode)
    {
        var normalizedLanguageCode = string.Equals(languageCode, "uk-UA", StringComparison.OrdinalIgnoreCase) ? "uk-UA" : "en";
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
