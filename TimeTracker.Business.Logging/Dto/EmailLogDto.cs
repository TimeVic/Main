namespace TimeTracker.Business.Logging.Dto;

public sealed class EmailLogDto
{
    public string EmailFrom { get; init; } = string.Empty;

    public string EmailTo { get; init; } = string.Empty;

    public string? EmailCc { get; init; }

    public string? EmailBcc { get; init; }

    public string EmailSubject { get; init; } = string.Empty;

    public string EmailBody { get; init; } = string.Empty;

    public string? EmailSendingError { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
