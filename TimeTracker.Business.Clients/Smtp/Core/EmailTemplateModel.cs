namespace TimeTracker.Business.Clients.Smtp.Core
{
    public class EmailTemplateModel
    {
        // this class is stored in a template cache

        public string BodyTemplate { get; set; } = string.Empty; // already includes the LAYOUT TEMPL + CONTENT TEMPL, both still have {placeholders}
        public string SubjectTemplate { get; set; } = string.Empty; // subject is extracted from CONTENT TEMPL <subject> tag
    }
}
