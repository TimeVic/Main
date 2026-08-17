using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using TimeTracker.Business.Clients.Smtp.Core;

namespace TimeTracker.Business.Notifications.Core
{
    public class EmailFactory
    {
        private static readonly ConcurrentDictionary<string, EmailTemplateModel> _cachedTemplates = new();

        public EmailTemplateModel GetEmailTemplate(string templateName, string languageCode)
        {
            var normalizedLanguageCode = NormalizeLanguageCode(languageCode);
            var cacheKey = $"{normalizedLanguageCode}/{templateName}";
            if (_cachedTemplates.TryGetValue(cacheKey, out var cachedTemplate))
            {
                return cachedTemplate;
            }

            var template = LoadEmailTemplate(templateName, normalizedLanguageCode);
            return _cachedTemplates.GetOrAdd(cacheKey, template);
        }

        public EmailBuilder GetEmailBuilder(string templateName, string languageCode)
        {
            var et = GetEmailTemplate(templateName, languageCode);
            var res = new EmailBuilder(et.BodyTemplate, et.SubjectTemplate);
            return res;
        }

        private string LoadFile(string templateName, string languageCode)
        {
            var assembly = GetType().Assembly;
            var resourceLanguageCode = languageCode.Replace('-', '_');
            var layoutResourcePath = $"{assembly.GetName().Name}.Templates.Emails.{resourceLanguageCode}.{templateName}";
            var resource = assembly.GetManifestResourceStream(layoutResourcePath);
            if (resource == null)
            {
                throw new Exception($"Email template wasn't found: '{templateName}'");
            }
            using (var reader = new StreamReader(resource))
            {
                return reader.ReadToEnd();
            }
        }

        private EmailTemplateModel LoadEmailTemplate(string templateName, string languageCode)
        {
            var contentTemplate = LoadFile(templateName, languageCode);
            string subjectTemplate = string.Empty;

            var subjectRegex = @"<!--\s*<subject>(?<subjectText>[^<]*)</subject>\s*-->";
            var subjectMatch = Regex.Match(contentTemplate, subjectRegex, RegexOptions.IgnoreCase);
            if (subjectMatch != null && subjectMatch.Groups["subjectText"].Success)
            {
                subjectTemplate = subjectMatch.Groups["subjectText"].Value;
                contentTemplate = Regex.Replace(
                    contentTemplate, 
                    subjectRegex, 
                    string.Empty, 
                    RegexOptions.IgnoreCase
                );
            }

            var res = new EmailTemplateModel();
            var layoutTemplate = LoadFile("_EmailLayout.htm", languageCode);
            res.BodyTemplate = layoutTemplate.Replace("{body}", contentTemplate);
            res.SubjectTemplate = subjectTemplate;

            return res;
        }

        private static string NormalizeLanguageCode(string? languageCode)
        {
            return string.Equals(languageCode, "uk-UA", StringComparison.OrdinalIgnoreCase) ? "uk-UA" : "en";
        }
    }
}
