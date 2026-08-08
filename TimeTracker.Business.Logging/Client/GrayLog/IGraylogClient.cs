using TimeTracker.Business.Logging.Dto;

namespace TimeTracker.Business.Logging.Client.GrayLog;

public interface IGraylogClient
{
    void LogEmail(EmailLogDto dto);
}
