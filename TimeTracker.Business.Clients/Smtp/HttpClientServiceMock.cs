using TimeTracker.Business.Clients.Smtp.Core;

using TimeTracker.Business.Logging.Client.GrayLog;
using TimeTracker.Business.Logging.Dto;

namespace TimeTracker.Business.Clients.Smtp;

public class SmtpClientServiceMock : ISmtpClientService
{
    private readonly IGraylogClient _graylogClient;

    public List<FakeEmailModel> SentMessages = new ();

    public SmtpClientServiceMock(IGraylogClient graylogClient)
    {
        _graylogClient = graylogClient;
    }
    public bool IsEmailSent
    {
        get => SentMessages.Any();
    }
        
    public void Reset()
    {
        SentMessages = new ();
    }

    public string SendEmail(string to, EmailBuilder emailBuilder, string? bcc)
    {
        emailBuilder.Build();
        return SendEmail("", to, emailBuilder.Subject, emailBuilder.Body, null, bcc);
    }

    public string SendEmail(string to, string subject, string body, string? bcc)
    {
        return SendEmail("", to, subject, body, null, bcc);
    }

    public string SendEmail(
        string? from, 
        string to, 
        string subject, 
        string body, 
        string? cc, 
        string? bcc
    )
    {
        SentMessages.Add(new FakeEmailModel(from, to, subject, body, cc, bcc));
        _graylogClient.LogEmail(new EmailLogDto
        {
            EmailFrom = from ?? string.Empty,
            EmailTo = to,
            EmailSubject = subject,
            EmailBody = body,
            EmailCc = cc,
            EmailBcc = bcc
        });
        return "";
    }
}
