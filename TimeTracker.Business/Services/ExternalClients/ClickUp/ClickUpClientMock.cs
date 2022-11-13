using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public class ClickUpClientMock: IClickUpClient
{
    public ICollection<TimeEntryEntity> SentTimeEntries = new List<TimeEntryEntity>();
    
    public bool IsSent => SentTimeEntries.Count > 0;

    public async Task<GetTaskResponseDto?> GetTaskAsync(TimeEntryEntity timeEntry)
    {
        await Task.CompletedTask;
        return new GetTaskResponseDto()
        {
            Name = "Some Task name",
            Description = "Some Task description",
            CustomId = timeEntry.TaskId
        };
    }

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
