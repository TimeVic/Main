using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public class ClickUpClientMock: IClickUpClient
{
    public ICollection<TimeEntryEntity> SentTimeEntries = new List<TimeEntryEntity>();

    public async Task<SetTimeEntryResponseDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        SentTimeEntries.Add(timeEntry);
        return new SetTimeEntryResponseDto()
        {
            Id = 123
        };
    }

    public void Reset()
    {
        
    }
}
