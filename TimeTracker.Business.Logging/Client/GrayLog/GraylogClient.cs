using TimeTracker.Business.Logging.Dto;

namespace TimeTracker.Business.Logging.Client.GrayLog;

public sealed class GraylogClient : IGraylogClient
{
    private const int MaxEmailBodyLength = 30_000;

    private readonly IGraylogGelfClient _graylogGelfClient;

    public GraylogClient(IGraylogGelfClient graylogGelfClient)
    {
        _graylogGelfClient = graylogGelfClient;
    }

    public void LogEmail(EmailLogDto dto)
    {
        var emailLog = dto.EmailBody.Length <= MaxEmailBodyLength
            ? dto
            : new EmailLogDto
            {
                EmailFrom = dto.EmailFrom,
                EmailTo = dto.EmailTo,
                EmailCc = dto.EmailCc,
                EmailBcc = dto.EmailBcc,
                EmailSubject = dto.EmailSubject,
                EmailBody = dto.EmailBody[..MaxEmailBodyLength],
                EmailSendingError = dto.EmailSendingError,
                CreatedAt = dto.CreatedAt
            };

        _graylogGelfClient.Send(
            $"Email message sending log: {emailLog.EmailTo}",
            [new KeyValuePair<string, object?>("EmailSendingLog", true)],
            emailLog
        );
    }
}
