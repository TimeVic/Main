using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace TimeTracker.Business.Notifications.Core.Emails
{
    class EmailFactory
    {
        // cached email templates
        private static readonly ConcurrentDictionary<string, EmailTemplateModel> _cachedTemplates = new();

        private string _layoutName = "_EmailLayout.htm"; // layout file name like "_EmailLayout.htm"
        private string _layoutTemplate; // cached content of the layout file

        public EmailFactory()
        {
            // load cached layout
            _layoutTemplate = LoadFile(_layoutName); // load this only once, on initialization
        }

        public EmailTemplateModel GetEmailTemplate(string templateName)
        {
            EmailTemplateModel res;
            if (_cachedTemplates.ContainsKey(templateName))
            {
                // from Cache
                _cachedTemplates.TryGetValue(templateName, out res);
            }
            else
            {
                // from File
                res = LoadEmailTemplate(templateName);
                // if some other thread inserted the value, it will ignore our value and return what other thread has inserted
                _cachedTemplates.GetOrAdd(templateName, res);
            }
            return res;
        }

        public EmailBuilder GetEmailBuilder(string templateName)
        {
            var et = GetEmailTemplate(templateName);
            var res = new EmailBuilder(et.BodyTemplate, et.SubjectTemplate);
            return res;
        }

        private string LoadFile(string templateName)
        {
            var assembly = GetType().Assembly;
            // TODO: Receive location from the config
            //var localeCode = LocalizationUtils.CultureCode().ToLower();
            var localeCode = "en";
            var layoutResourcePath = $"{assembly.GetName().Name}.Templates.Emails.{localeCode}.{templateName}";
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

        private EmailTemplateModel LoadEmailTemplate(string templateName)
        {
            var contentTemplate = LoadFile(templateName);
            string subjectTemplate = string.Empty; // it is OK to return it empty, that would save some stringBuilder object in EmailBuilder object

            // SUBJECT TEMPLATE
            // it is stored in the content template file in the following format:
            // <!-- <subject>This is email subject, {placeholders} blah blah </subject> -->
            // now we need to extract SUBJECT TEMPLATE into its own string, and remove it from contentTemplate

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

            // return results
            var res = new EmailTemplateModel();
            res.BodyTemplate = _layoutTemplate.Replace("{body}", contentTemplate); // merge layout and content
            res.SubjectTemplate = subjectTemplate;

            return res;
        }
    }
}
