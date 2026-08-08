using TimeTracker.Business.Logging.Dto;

namespace TimeTracker.Business.Logging.Client.GrayLog;

public sealed class GraylogClientMock : IGraylogClient
{
    public ICollection<EmailLogDto> EmailLogs = new List<EmailLogDto>();

    public void Clear()
    {
        EmailLogs.Clear();
    }

    public void LogEmail(EmailLogDto dto)
    {
        EmailLogs.Add(dto);
    }
}
